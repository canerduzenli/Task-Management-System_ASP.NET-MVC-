using X.PagedList;

namespace Assignment_2.Models
{
    public class TicketVM
    {
        public Project Project { get; set; }
        public IPagedList<Ticket> Tickets { get; set; }
    }
}
