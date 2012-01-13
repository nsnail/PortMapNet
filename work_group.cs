using System.Net;

namespace portmap_net
{
    internal struct work_group
    {
        #region Data Members (4)

        public int _id;
        public EndPoint _point_in;
        public string _point_out_host;
        public ushort _point_out_port;

        #endregion Data Members
    }
}
