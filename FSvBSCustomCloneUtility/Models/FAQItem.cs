using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSvBSCustomCloneUtility.Models {
    /// <summary>
    /// An item in the FAQ list
    /// </summary>
    public class FAQItem {
        /// <summary>
        /// Gets or sets the item's question
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets the item's answer
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Instantiate a new FAQ item
        /// </summary>
        /// <param name="question">Question</param>
        /// <param name="answer">Answer</param>
        public FAQItem(string question, string answer) {
            Question = question;
            Answer = answer;
        }
    }
}
