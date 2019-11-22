using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models
{
    public class TodoApiContext : DbContext
    {
        public TodoApiContext (DbContextOptions<TodoApiContext> options)
            : base(options)
        {
        }

        public DbSet<TodoApi.Models.TodoItem> TodoItem { get; set; }
    }
}
