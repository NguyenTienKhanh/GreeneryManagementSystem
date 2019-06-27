using GMS.Models.Entities.Collections;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMS.Models.Repositories
{
    public class TreeHistoryRepository : IRepository
    {
        private IMongoCollection<TreeHistory> repos;

        public TreeHistoryRepository()
        {
            repos = db.GetCollection<TreeHistory>("TreeHistory");
        }

        //Find
        public async Task<List<TreeHistory>> FindAll()
        {
            return await repos.AsQueryable<TreeHistory>().ToListAsync();
        }
  
        public async Task<TreeHistory> Find(string id)
        {
            IAsyncCursor<TreeHistory> cursor = await repos
                .FindAsync(Builders<TreeHistory>.Filter.Eq("_id", new ObjectId(id)));
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<List<TreeHistory>> FindByAttr(string options, string name)
        {
            IAsyncCursor<TreeHistory> cursor = await repos
                .FindAsync(Builders<TreeHistory>.Filter.Eq(options, name));
            return await cursor.ToListAsync();
        }

        public async Task<List<TreeHistory>> FindByRangeDate(DateTime from, DateTime to)
        {
            var list = await FindAll();
            var result = new List<TreeHistory>();
            foreach (var item in list)
            {
                if (item.Date >= from && item.Date <= to)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public async Task<List<TreeHistory>> FindByRangeDateWithStatus(DateTime from, DateTime to, string status)
        {
            var list = await FindAll();
            var result = new List<TreeHistory>();
            foreach (var item in list)
            {
                if (item.Date >= from && item.Date <= to && item.Name.Equals(status))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public async Task<List<TreeHistory>> FindByDate(DateTime date)
        {
            var list = await FindAll();
            var result = new List<TreeHistory>();
            foreach (var item in list)
            {
                if (item.Date == date)
                {
                    result.Add(item);
                }
            }
            return result;
        }


        //Create
        public async Task<TreeHistory> Create(TreeHistory history)
        {
            await repos.InsertOneAsync(history);
            return history;
        }

        //Update
        public async Task<Boolean> Update(TreeHistory history)
        {
            if (history == null)
                return false;
            UpdateResult result = await repos.UpdateOneAsync(
                Builders<TreeHistory>.Filter.Eq("_id", new ObjectId(history.Id)),
                Builders<TreeHistory>.Update
                .Set("name", history.Name)
                .Set("userId", history.UserId)
                .Set("date", history.Date)
                .Set("detail", history.Detail)
                .Set("treeId", history.TreeId)
                .Set("polygonId", history.PolygonId)
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
            DeleteResult result = await repos
                 .DeleteOneAsync(Builders<TreeHistory>.Filter.Eq("_id", id));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<TreeHistory> cursor = await repos
                .FindAsync(Builders<TreeHistory>.Filter.Eq("_id", id));
            return await cursor.AnyAsync();
        }
    }
}
