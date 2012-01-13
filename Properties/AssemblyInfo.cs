using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using portmap_net;

// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("端口映射器 " + program.product_version + " " + program.product_version_addl)]
[assembly: AssemblyDescription("将TCP端口映射到另一台主机")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("www.beta-1.cn")]
[assembly: AssemblyProduct("PortMapNet")]
[assembly: AssemblyCopyright("Copyright \u00a9 2009-2012 beta-1.cn")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("3f2b0910-65a4-4a58-932e-1be77ec65b22")]

// 程序集的版本信息由下面四个值组成:
//
//      主版本
//      次版本 
//      内部版本号
//      修订号
//
// 可以指定所有这些值，也可以使用“内部版本号”和“修订号”的默认值，
// 方法是按如下所示使用“*”:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.1.113.0")]
[assembly: AssemblyFileVersion("0.1.113.0")]
[assembly: log4net.Config.XmlConfigurator(ConfigFileExtension = "config", Watch = true)]