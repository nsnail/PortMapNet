using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using log4net;

namespace portmap_net
{
    internal class program
    {
        #region Fields (4)

        private static StringBuilder _console_buf = new StringBuilder();
        private static int _id_plus = 0;
        private static readonly ILog _l4n = LogManager.GetLogger(typeof(program));
        private static readonly Dictionary<int, stat_obj> _stat_info = new Dictionary<int, stat_obj>();
        public const string product_version = "0.1.113";
        public const string product_version_addl = "beta";
        #endregion Fields

        #region Methods (8)

        // Private Methods (8) 

        private static List<work_group> load_maps_cfg()
        {
            string maps_cfg = ConfigurationManager.AppSettings["portmaps"];
            if (string.IsNullOrEmpty(maps_cfg))
                throw new Exception("配置文件错误: 缺少PortMap配置");

            string[] tmp1 = maps_cfg.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (tmp1.Length == 0)
                throw new Exception("配置文件错误: 缺少PortMap配置");

            List<work_group> rtn = new List<work_group>();
            foreach (string tmp2 in tmp1)
            {
                string[] tmp3 = tmp2.Split(new[] { '|' });
                if (tmp3.Length != 2)
                    throw new Exception("配置文件错误: 每组PortMap配置必须为2个节点");
                work_group rtn_item = new work_group { _id = ++_id_plus };
                for (int i = 0; i != 2; ++i)
                {
                    string[] tmp4 = tmp3[i].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tmp4.Length != 2)
                        throw new Exception("配置文件错误: IP节点格式错误");
                    IPAddress ip = IPAddress.Any;
                    if (i == 0 && tmp4[0] != "*" && !IPAddress.TryParse(tmp4[0], out ip))
                        throw new Exception("配置文件错误: IP节点格式错误");
                    ushort port;
                    if (!ushort.TryParse(tmp4[1], out port))
                        throw new Exception("配置文件错误: IP节点格式错误");
                    if (i == 0)
                        rtn_item._point_in = new IPEndPoint(ip, port);
                    if (i == 1)
                    {
                        rtn_item._point_out_host = tmp4[0];
                        rtn_item._point_out_port = port;
                    }
                }
                rtn.Add(rtn_item);
            }
            return rtn;
        }

        private static void Main(string[] args)
        {
            List<work_group> maps_list;
            try
            {
                maps_list = load_maps_cfg();
            }
            catch (Exception exp)
            {
                Console.WriteLine(program_ver);
                Console.WriteLine(exp.Message);
                system("pause");
                return;
            }
            foreach (var map_item in maps_list)
            {
                map_start(map_item);
            }

            Console.CursorVisible = false;
            while (true)
            {
                show_stat();
                Thread.Sleep(2000);
            }
        }

        private static void map_start(work_group work)
        {
            Socket sock_svr = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            bool start_error = false;
            try
            {
                sock_svr.Bind(work._point_in);
                sock_svr.Listen(10);
                sock_svr.BeginAccept(on_local_connected, new object[] { sock_svr, work });
            }
            catch (Exception exp)
            {
                _l4n.Error(exp.Message);
                start_error = true;
            }
            finally
            {
                _stat_info.Add(work._id, new stat_obj(work._point_in.ToString(), work._point_out_host + ":" + work._point_out_port, !start_error, 0, 0, 0));
            }
        }

        private static void on_local_connected(IAsyncResult ar)
        {
            object[] ar_arr = ar.AsyncState as object[];
            Socket sock_svr = ar_arr[0] as Socket;
            work_group work = (work_group)ar_arr[1];

            ++_stat_info[work._id]._connect_cnt;
            Socket sock_cli = sock_svr.EndAccept(ar);
            sock_svr.BeginAccept(on_local_connected, ar.AsyncState);
            Socket sock_cli_remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sock_cli_remote.Connect(work._point_out_host, work._point_out_port);
            }
            catch (Exception exp)
            {
                _l4n.Warn(exp.Message);
                try
                {
                    sock_cli.Shutdown(SocketShutdown.Both);
                    sock_cli_remote.Shutdown(SocketShutdown.Both);
                    sock_cli.Close();
                    sock_cli_remote.Close();
                }
                catch (Exception) { }
                --_stat_info[work._id]._connect_cnt;
                return;
            }
            Thread t_send = new Thread(new ParameterizedThreadStart(recv_and_send_caller)) { IsBackground = true };
            Thread t_recv = new Thread(new ParameterizedThreadStart(recv_and_send_caller)) { IsBackground = true };
            t_send.Start(new object[] { sock_cli, sock_cli_remote, work._id, true });
            t_recv.Start(new object[] { sock_cli_remote, sock_cli, work._id, false });
            t_send.Join();
            t_recv.Join();
            --_stat_info[work._id]._connect_cnt;
        }

        private static void recv_and_send(Socket from_sock, Socket to_sock, Action<int> send_complete)
        {
            byte[] recv_buf = new byte[4096];
            int recv_len;
            while ((recv_len = from_sock.Receive(recv_buf)) > 0)
            {
                to_sock.Send(recv_buf, 0, recv_len, SocketFlags.None);
                send_complete(recv_len);
            }
        }

        private static void recv_and_send_caller(object thread_param)
        {
            object[] param_arr = thread_param as object[];
            Socket sock1 = param_arr[0] as Socket;
            Socket sock2 = param_arr[1] as Socket;
            try
            {
                recv_and_send(sock1, sock2, bytes =>
                {
                    stat_obj stat = _stat_info[(int)param_arr[2]];
                    if ((bool)param_arr[3])
                        stat._bytes_send += bytes;
                    else
                        stat._bytes_recv += bytes;
                });
            }
            catch (Exception exp)
            {
                _l4n.Info(exp.Message);
                try
                {
                    sock1.Shutdown(SocketShutdown.Both);
                    sock2.Shutdown(SocketShutdown.Both);
                    sock1.Close();
                    sock2.Close();
                }
                catch (Exception) { }
            }
        }

        private static void show_stat()
        {
            StringBuilder curr_buf = new StringBuilder();
            curr_buf.AppendLine(program_ver);
            curr_buf.AppendLine(stat_obj._print_head);
            foreach (KeyValuePair<int, stat_obj> item in _stat_info)
            {
                curr_buf.AppendLine(item.Value.ToString());
            }
            if (_console_buf.Equals(curr_buf))
                return;
            Console.Clear();
            Console.WriteLine(curr_buf);
            _console_buf = curr_buf;
        }

        [DllImport("msvcrt.dll")]
        private static extern bool system(string str);

        #endregion Methods

        private const string program_ver = @"[PortMapNet(0.1)  http://www.beta-1.cn]
--------------------------------------------------";
    }
}
