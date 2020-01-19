using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PowerBIEmbedded_AppOwnsData.Models
{
    public class ReportDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ReportDefinition(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public ReportDefinition()
        {

        }
    }
}