﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiteDB
{
    public partial class Collection<T>
    {
        public T FindById(object id)
        {
            var col = this.GetCollectionPage();

            var node = _engine.Indexer.FindOne(col.PK, id);

            if (node == null) return default(T);

            var dataBlock = _engine.Data.Read(node.DataBlock, true);

            return new BsonDocument(dataBlock.Data).To<T>();
        }

        public T FindOne(Query query)
        {
            return this.Find(query).FirstOrDefault();
        }

        /// <summary>
        /// Find objects inside a collection using a index. Index must exists
        /// </summary>
        public IEnumerable<T> Find(Query query)
        {
            var col = this.GetCollectionPage();

            var nodes = query.Execute(_engine, col); 

            foreach (var node in nodes)
            {
                var dataBlock = _engine.Data.Read(node.DataBlock, true);

                var doc = new BsonDocument(dataBlock.Data);

                yield return doc.To<T>();
            }
        }

        /// <summary>
        /// Find all object ids in a collection using a index. Index must exists
        /// </summary>
        /// <typeparam name="TKey">Type of Id value</typeparam>
        public IEnumerable<TKey> FindIds<TKey>(Query query)
        {
            var col = this.GetCollectionPage();

            var nodes = query.Execute(_engine, col);

            foreach (var node in nodes)
            {
                yield return (TKey)node.Key.Value;
            }
        }

        /// <summary>
        /// Get object count using property on collection.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            var col = this.GetCollectionPage();

            return Convert.ToInt32(col.DocumentCount);
        }

        /// <summary>
        /// Count objects with a query. Do not read objects
        /// </summary>
        public int Count(Query query)
        {
            var col = this.GetCollectionPage();

            return query.Execute(_engine, col).Count();
        }

        /// <summary>
        /// Returns true if query returns any object. Do not read objects
        /// </summary>
        public bool Exists(Query query)
        {
            var col = this.GetCollectionPage();

            return query.Execute(_engine, col).FirstOrDefault() != null;
        }
    }
}
