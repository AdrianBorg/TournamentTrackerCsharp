using System;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Represents one team in a matchup
    /// </summary>
    public class MatchupEntryModel
    {
        public int Id { get; set; }
        /// <summary>
        /// Represents one team in the matchup
        /// </summary>
        public TeamModel TeamCompeting { get; set; }

        /// <summary>
        /// The ID from the database that will be used to identify the competing team
        /// </summary>
        public int TeamCompetingId { get; set; }

		/// <summary>
		/// Represents the score for this particular team
		/// </summary>
		public double Score { get; set; }

        /// <summary>
        /// Represents the matchup that this team came 
        /// from as the winner
        /// </summary>
        public MatchupModel ParentMatchup { get; set; }

		/// <summary>
		/// The ID from the database that will be used to identify the parent matchup
		/// </summary>
		public int ParentMatchupId { get; set; }
	}
}