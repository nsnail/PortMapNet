
using System;
namespace portmap_net
{
    internal class stat_obj
    {
        #region Fields (5)

        public int _connect_cnt;
        public string _point_in;
        public string _point_out;
        public const string _print_head = "输入IP              输出IP              状态    连接数    接收/发送";
        public bool _running;
        public long _bytes_send;
        public long _bytes_recv;

        #endregion Fields

        #region Constructors (1)

        public stat_obj(string point_in, string point_out, bool running, int connect_cnt, int bytes_send, int bytes_recv)
        {
            _point_in = point_in;
            _point_out = point_out;
            _running = running;
            _connect_cnt = connect_cnt;
            _bytes_recv = bytes_recv;
            _bytes_send = bytes_send;
        }

        #endregion Constructors

        #region Methods (1)

        // Public Methods (1) 

        public override string ToString()
        {
            return string.Format("{0}{1}{2}{3}{4}", _point_in.PadRight(20, ' '), _point_out.PadRight(20, ' '), (_running ? "运行中  " : "启动失败"), _connect_cnt.ToString().PadRight(10, ' '), Math.Round((double)_bytes_recv / 1024) + "k/" + Math.Round((double)_bytes_send / 1024) + "k");
        }

        #endregion Methods
    }
}
