using System.Collections.Generic;
using System.Linq;
using Td.Kylin.DataCache.CacheModel;
using Td.Kylin.DataCache.Context;
using Td.Kylin.DataCache.IServices;

namespace Td.Kylin.DataCache.Services
{
    internal sealed class ForumCircleService<DbContext> : IForumCircleService where DbContext : DataContext, new()
    {
        public List<ForumCircleCacheModel> GetEnabledAll()
        {
            using (var db = new DbContext())
            {
                var query = from p in db.Circle_Forum
                            where p.IsDelete == false && p.Disabled == false
                            orderby p.OperatorNumber descending
                            select new ForumCircleCacheModel
                            {
                                CategoryID = p.CategoryID,
                                Description = p.Description,
                                ForumID = p.ForumID,
                                ForumName = p.ForumName,
                                Logo = p.Logo,
                                PassLevel = p.PassLevel,
                                PostLevel = p.PostLevel,
                                PostType = p.PostType
                            };

                return query.ToList();
            }
        }
    }
}
