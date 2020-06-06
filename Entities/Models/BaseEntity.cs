using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clipp.Server.Entities.Models
{
    public class BaseEntity
    {
        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastModifiedOn { get; set; }

        public virtual string CreatedBy { get; set; }

        public string LastModifiedBy { get; set; }
    }
}
