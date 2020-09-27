using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
	public static class TournamentLogic
	{
		// Order our list of teams randomly
		// Check if it is big enough - if not, add in byes
		// Create out first round of matchups
		// Create every round after that

		public static void CreateRounds(TournamentModel model)
		{
			List<TeamModel> randomizedTeams = RandomizedTeamOrder(model.EnteredTeams);
			int rounds = FindNumberOfRounds(randomizedTeams.Count);
			int byes = NumberOfByes(rounds, randomizedTeams.Count);

			model.Rounds.Add(CreateFirstRound(byes, randomizedTeams));

			CreateOtherRounds(model, rounds);
		}

		public static void UpdateTournamentResults(TournamentModel model)
		{
			List<MatchupModel> toScore = new List<MatchupModel>();

			model.Rounds.ForEach(round =>
			{
				round.ForEach(rm =>
				{
					if (
					//rm.Winner == null && 
						(rm.Entries.Any(x => x.Score != 0) 
							|| rm.Entries.Count == 1))
					{
						toScore.Add(rm);
					}
				});
			});

			MarkWinnersInMatchups(toScore);

			AdvanceWinners(toScore, model);

			toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
		}

		private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
		{
			models.ForEach(m =>
			{
				tournament.Rounds.ForEach(round =>
				{
					round.ForEach(rm =>
					{
						rm.Entries.ForEach(me =>
						{
							if (me.ParentMatchup != null)
							{
								if (me.ParentMatchup.Id == m.Id)
								{
									me.TeamCompeting = m.Winner;
									GlobalConfig.Connection.UpdateMatchup(rm);
								}
							}
						});
					});
				});
			});
		}

		private static void MarkWinnersInMatchups(List<MatchupModel> models)
		{
			// greater or lesser
			string greaterWins = ConfigurationManager.AppSettings["greaterWins"];

			foreach (MatchupModel m in models)
			{
				// Checks for bye week
				if (m.Entries.Count == 1)
				{
					m.Winner = m.Entries[0].TeamCompeting;
					continue;
				}
				// 0 means false or low score wins
				if (greaterWins == "0")
				{
					if (m.Entries[0].Score < m.Entries[1].Score)
					{
						m.Winner = m.Entries[0].TeamCompeting;
					}
					else if (m.Entries[1].Score < m.Entries[0].Score)
					{
						m.Winner = m.Entries[1].TeamCompeting;
					}
					else
					{
						throw new Exception("We do not allow ties in this app");
					}
				}
				else // 1 means true or high score wins
				{
					if (m.Entries[0].Score > m.Entries[1].Score)
					{
						m.Winner = m.Entries[0].TeamCompeting;
					}
					else if (m.Entries[1].Score > m.Entries[0].Score)
					{
						m.Winner = m.Entries[1].TeamCompeting;
					}
					else
					{
						throw new Exception("We do not allow ties in this app");
					}
				}
			}
		}

		private static void CreateOtherRounds(TournamentModel model, int rounds)
		{
			int round = 2;
			List<MatchupModel> previousRound = model.Rounds[0];
			List<MatchupModel> currRound = new List<MatchupModel>();
			MatchupModel currMatchup = new MatchupModel();

			while (round <= rounds)
			{
				previousRound.ForEach(match =>
				{
					currMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match });

					if (currMatchup.Entries.Count > 1)
					{
						currMatchup.MatchupRound = round;
						currRound.Add(currMatchup);
						currMatchup = new MatchupModel();
					}
				});

				model.Rounds.Add(currRound);
				previousRound = currRound;

				currRound = new List<MatchupModel>();
				round += 1;
			}
		}

		private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
		{
			List<MatchupModel> output = new List<MatchupModel>();
			MatchupModel curr = new MatchupModel();

			teams.ForEach(team =>
			{
				curr.Entries.Add(new MatchupEntryModel { TeamCompeting = team });

				if (byes > 0 || curr.Entries.Count > 1)
				{
					curr.MatchupRound = 1;
					output.Add(curr);
					curr = new MatchupModel();

					if (byes > 0)
					{
						byes -= 1;
					}
				}
			});

			return output;
		}

		private static int NumberOfByes(int rounds, int numberOfTeams)
		{
			int output = 0;
			int totalTeams = 1;

			for (int i = 1; i <= rounds ; i++)
			{
				totalTeams *= 2;
			}

			output = totalTeams - numberOfTeams;

			return output;
		}

		private static int FindNumberOfRounds(int teamCount)
		{
			int output = 1;
			int val = 2;

			while (val < teamCount)
			{
				output += 1;
				val *= 2;
			}

			return output;
		}

		private static List<TeamModel> RandomizedTeamOrder(List<TeamModel> teams)
		{
			return teams.OrderBy(t => Guid.NewGuid()).ToList();
		}
	}
}
