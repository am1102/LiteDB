﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace LiteDB
{
    /// <summary>
    /// Represets a file inside files collection
    /// </summary>
    public class FileEntry
    {
        public string Id { get; set; }
        public int Length { get; set; }
        public DateTime UploadDate { get; set; }
        public NameValueCollection Metadata { get; set; }

        internal uint PageID { get; set; }

        internal FileEntry(string id, NameValueCollection metadata)
        {
            this.PageID = uint.MaxValue;
            this.Id = id;
            this.Metadata = metadata == null ? new NameValueCollection() : metadata;
            this.UploadDate = DateTime.Now;
        }

        internal FileEntry(BsonDocument doc)
        {
            this.Id = doc["Id"].AsString;
            this.Length = doc["Length"].AsInt;
            this.UploadDate = doc["UploadDate"].AsDateTime;
            this.Metadata = (NameValueCollection)doc["Metadata"].RawValue;
            this.PageID = (uint)doc["PageID"].RawValue;
        }

        internal BsonDocument ToBson()
        {
            var doc = new BsonDocument();

            doc["Id"] = new BsonValue(this.Id);
            doc["Length"] = new BsonValue(this.Length);
            doc["UploadDate"] = new BsonValue(this.UploadDate);
            doc["Metadata"] = new BsonValue(this.Metadata);
            doc["PageID"] = new BsonValue(this.PageID);

            return doc;
        }

        /// <summary>
        /// Open file stream to read from database
        /// </summary>
        public LiteFileStream OpenRead(LiteEngine db)
        {
            return new LiteFileStream(db, this);
        }

        /// <summary>
        /// Save file content to a external file
        /// </summary>
        public void SaveAs(LiteEngine db, string filename, bool overwritten = true)
        {
            using (var file = new FileStream(filename, overwritten ? FileMode.Create : FileMode.CreateNew))
            {
                this.OpenRead(db).CopyTo(file);
            }
        }
    }
}
