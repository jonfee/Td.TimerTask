using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KylinService.Core
{
    /// <summary>
    /// 配置项
    /// </summary>
    public class Configs
    {
        /// <summary>
        /// Kylin项目数据库连接字符串
        /// </summary>
        public static string KylinConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["KylinConnectionString"].ConnectionString;
            }
        }
    }
}
