using GMS.Models.Entities.Collections;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMS.Models.Repositories
{
    public class TreeNameRepository : IRepository
    {
        private IMongoCollection<TreeName> TreeNameCollection;

        public TreeNameRepository()
        {
            TreeNameCollection = db.GetCollection<TreeName>("TreeName");
        }
        
        //Find
        public async Task<List<TreeName>> FindAll()  
        {
            var list = await TreeNameCollection.AsQueryable<TreeName>().ToListAsync();
            list.Sort(delegate (TreeName x, TreeName y)
            {
                return x.Name.CompareTo(y.Name);
            });
            return list;
        }

        public async Task<TreeName> Find(string id)
        {
            IAsyncCursor<TreeName> cursor = await TreeNameCollection
                .FindAsync(Builders<TreeName>.Filter.Eq("_id", new ObjectId(id)));
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<TreeName> FindByName(string name)
        {
            IAsyncCursor<TreeName> cursor = await TreeNameCollection
                .FindAsync(Builders<TreeName>.Filter.Eq("name", name));
            return await cursor.FirstOrDefaultAsync();
        }

        //Create
        public async Task<TreeName> Create(TreeName treeName)
        {
            await TreeNameCollection.InsertOneAsync(treeName);
            return treeName;
        }

        //Update
        public async Task<Boolean> Update(TreeName treeName)
        {
            if (treeName == null)
                return false;
            UpdateResult result = await TreeNameCollection.UpdateOneAsync(
                Builders<TreeName>.Filter.Eq("_id", new ObjectId(treeName.Id)),
                Builders<TreeName>.Update
                .Set("name", treeName.Name)
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
            DeleteResult result = await TreeNameCollection
                 .DeleteOneAsync(Builders<TreeName>.Filter.Eq("_id", new ObjectId(id)));
            
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<TreeName> cursor = await TreeNameCollection
                .FindAsync(Builders<TreeName>.Filter.Eq("_id", new ObjectId(id)));
            return await cursor.AnyAsync();
        }
    }
}
