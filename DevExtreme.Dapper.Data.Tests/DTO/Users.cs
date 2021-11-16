using DevExtreme.Dapper.Data.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DevExtreme.Dapper.Data.Tests.DTO
{
    [Table("Users", Schema = "dbo")]
    public class Users
    {
        [Key]
        public int Id;

        [DapperNotWhere]
        public string FirstName;

        [DapperNotSelect]
        public string LastName;

        [DapperOperations("<>", "=")]
        public string Password;

        public DateTime CreateDate;
    }
}
