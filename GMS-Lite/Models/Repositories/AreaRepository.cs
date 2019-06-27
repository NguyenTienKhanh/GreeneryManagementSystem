using GMS.Models.Entities.Collections;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMS.Models.Repositories
{
    public class AreaRepository : IRepository
    {
        private IMongoCollection<Area> AreaCollection;

        public AreaRepository()
        {
            AreaCollection = db.GetCollection<Area>("Area");
        }

        //Find
        public async Task<List<Area>> FindAll()
        {
            IAsyncCursor<Area> cursor;
            cursor = await AreaCollection
            .FindAsync<Area>(area => !area.IsDeleted);

            return await cursor.ToListAsync();
        }

        public async Task<List<string>> GetAllTreeID(string id)
        {
            if (id == null)
            {
                return null;
            }

            Area area = await Find(id);

            if (area == null)
            {
                return null;
            }

            return area.Trees;
        }

        public async Task<Area> Find(string id)
        {
            IAsyncCursor<Area> cursor = await AreaCollection
                .FindAsync(Builders<Area>.Filter.Eq("_id", new ObjectId(id)));
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<List<Area>> FindByAttr(string attr, string options)
        {
            IAsyncCursor<Area> cursor = await AreaCollection
                .FindAsync(Builders<Area>.Filter.Eq(options, attr));
            return await cursor.ToListAsync();
        }

        //Create
        public async Task<Area> Create(Area Area)
        {
            await AreaCollection.InsertOneAsync(Area);
            return Area;
        }

        //add tree id
        public async Task<bool> AddTreeId(string areaId, string treeId)
        {
            Area area = await Find(areaId);
            area.Trees.Add(treeId);
            return await Update(area);
        }

        //delete tree id
        public async Task<bool> DeleteTreeId(string areaId, string treeId)
        {
            Area area = await Find(areaId);

            if (area == null)
            {
                return false;
            }

            if (!area.Trees.Remove(treeId))
            {
                return false;
            }
            return await Update(area);
        }

        //Update
        public async Task<Boolean> Update(Area Area)
        {
            if (Area == null)
                return false;
            var old = await Find(Area.Id);
            UpdateResult result = await AreaCollection.UpdateOneAsync(
                Builders<Area>.Filter.Eq("_id",new ObjectId(Area.Id)),
                Builders<Area>.Update
                .Set("code",Area.Code)
                .Set("name", Area.Name)
                .Set("isDeleted", Area.IsDeleted)
                .Set("polygon", Area.Polygon)
                .Set("center", Area.Center)
                .Set("farthest", Area.Farthest)
                .Set("managerId", Area.ManagerId)
                .Set("trees", Area.Trees)
                );
            if (result.IsAcknowledged)
            {
                return result.MatchedCount > 0 && result.ModifiedCount > 0;
            }
            return false;
        }

        //Delete
        public async Task<Boolean> Delete(string id)
        {
            DeleteResult result = await AreaCollection
                 .DeleteOneAsync(Builders<Area>.Filter.Eq("_id", id));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<Area> cursor = await AreaCollection
                .FindAsync(Builders<Area>.Filter.Eq("_id", id));
            return await cursor.AnyAsync();
        }

    }
}
