using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class MessageDto
    {
        public Guid Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
