using AO.Models;
using AO.Models.Enums;
using AO.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace DbFlow.Models.Conventions
{
    [Identity(nameof(Id))]
    public abstract class BaseTable : IAudit
    {
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string CreatedBy { get; set; }

        public DateTime DateCreated { get; set; }

        [MaxLength(50)]
        public string ModifiedBy { get; set; }

        public DateTime? DateModified { get; set; }

        public void Stamp(SaveAction saveAction, IUserBase user)
        {
            switch (saveAction)
            {
                case SaveAction.Insert:
                    CreatedBy = user.Name;
                    DateCreated = user.LocalTime;
                    break;

                case SaveAction.Update:
                    ModifiedBy = user.Name;
                    DateModified = user.LocalTime;
                    break;
            }
        }

        public override bool Equals(object obj)
        {
            var baseTable = obj as BaseTable;
            return (baseTable != null) ? baseTable.Id == Id : false;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}
