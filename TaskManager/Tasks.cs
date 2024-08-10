using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager
{
    class Tasks
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string About { get; set; }
        public System.DateTime Start { get; set; }
        public System.DateTime Finish { get; set; }
        public int StatusId { get; set; }
        public int UserId { get; set; }
    }
}
