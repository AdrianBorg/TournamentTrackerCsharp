using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Represents one Person
    /// </summary>
    public class PersonModel
    {

        /// <summary>
        /// The unique identifier for the prize.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The first name of the person
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// The last name of the person
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// The primary email of the person
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// The primary cell phone number of the person
        /// </summary>
        public string CellphoneNumber { get; set; }

        public string FullName
        {
            get
            {
                return $"{ FirstName } { LastName }";
            }
        }
    }
}
