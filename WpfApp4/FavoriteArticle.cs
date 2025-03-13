using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4
{
    public class FavoriteArticle
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime AddedAt { get; set; }
        public string Category { get; set; }
    }
}
