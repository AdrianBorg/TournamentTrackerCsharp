using System.Collections.Generic;

namespace TrackerLibrary.Models
{
    /// <summary>
    /// Represents one match in th tournament
    /// </summary>
    public class MatchupModel
    {
        public int Id { get; set; }
        /// <summary>
        /// The set of teams that were involved
        /// </summary>
        public List<MatchupEntryModel> Entries { get; set; } = new List<MatchupEntryModel>();
       
        /// <summary>
        /// The winner of the match
        /// </summary>
        public TeamModel Winner { get; set; }

        /// <summary>
        /// The ID from the database that will be used to identify the winner
        /// </summary>
		public int WinnerId { get; set; }

		/// <summary>
		/// Which round this match is a part of
		/// </summary>
		public int MatchupRound { get; set; }

		public string DisplayName {
            get
            {
                string output = "";
                Entries.ForEach(me =>
                {
					if (me.TeamCompeting != null)
					{
						if (output.Length == 0)
						{
							output = me.TeamCompeting.TeamName;
						}
						else
						{
							output += $" vs. { me.TeamCompeting.TeamName }";
						}
					}
					else
					{
                        output = "Matchup not yet determined";
                        return;
					}
                });
                return output;
            } 
        }
	}
}