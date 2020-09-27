using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
	public static class TextConnectorProcessor
	{
		public static string FullFilePath(this string filename)
		{
			return $"{ ConfigurationManager.AppSettings["filePath"] }\\{ filename }";
		}

		public static List<string> LoadFile(this string file)
		{
			if (!File.Exists(file))
			{
				return new List<string>();
			}

			return File.ReadAllLines(file).ToList();
		}

		public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
		{
			List<PrizeModel> output = new List<PrizeModel>();

			lines.ForEach(line =>
			{
				string[] cols = line.Split(',');
				PrizeModel p = new PrizeModel();
				p.Id = int.Parse(cols[0]);
				p.PlaceNumber = int.Parse(cols[1]);
				p.PlaceName = cols[2];
				p.PrizeAmount = decimal.Parse(cols[3]);
				p.PrizePercentage = double.Parse(cols[4]);
				output.Add(p);
			});

			return output;
		}

		public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
		{
			List<PersonModel> output = new List<PersonModel>();

			lines.ForEach(line =>
			{
				string[] cols = line.Split(',');
				PersonModel p = new PersonModel();
				p.Id = int.Parse(cols[0]);
				p.FirstName = cols[1];
				p.LastName = cols[2];
				p.Email = cols[3];
				p.CellphoneNumber = cols[4];
				output.Add(p);
			});

			return output;
		}

		public static List<TeamModel> ConvertToTeamModels(this List<string> lines)
		{
			List<TeamModel> output = new List<TeamModel>();
			List<PersonModel> people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();

			lines.ForEach(line =>
			{
				string[] cols = line.Split(',');
				TeamModel t = new TeamModel();
				t.Id = int.Parse(cols[0]);
				t.TeamName = cols[1];

				string[] personIds = cols[2].Split('|');

				foreach (string id in personIds)
				{
					t.TeamMembers.Add(people.Where(x => x.Id == int.Parse(id)).First());
				}

				output.Add(t);
			});

			return output;
		}

		public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines)
		{
			//id,tournamentName,EntryFee,(id|id|id - Entered Teams),(id|id|id - Prizes),(Rounds - id^id^id|id^id^id|id^id^id)
			List<TournamentModel> output = new List<TournamentModel>();
			List<TeamModel> teams = GlobalConfig.TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();
			List<PrizeModel> prizes = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();
			List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();


			lines.ForEach(line =>
			{
				string[] cols = line.Split(',');
				TournamentModel tm = new TournamentModel();
				tm.Id = int.Parse(cols[0]);
				tm.TournamentName = cols[1];
				tm.EntryFee = decimal.Parse(cols[2]);

				string[] teamIds = cols[3].Split('|');

				foreach(string id in teamIds)
				{
					tm.EnteredTeams.Add(teams.Where(x => x.Id == int.Parse(id)).First());
				}

				if (cols[4].Length > 0)
				{
					string[] prizeIds = cols[4].Split('|');

					foreach (string id in prizeIds)
					{
						tm.Prizes.Add(prizes.Where(x => x.Id == int.Parse(id)).First());
					} 
				}

				// Capture Rounds info
				string[] rounds = cols[5].Split('|');

				rounds.ToList().ForEach(round =>
				{
					List< MatchupModel> ms = new List<MatchupModel>();
					string[] msText = round.Split('^');

					msText.ToList().ForEach(matchupModelTextId =>
					{
						ms.Add(matchups.Where(x => x.Id == int.Parse(matchupModelTextId)).First());
					});

					tm.Rounds.Add(ms);
				});

				output.Add(tm);
			});

			return output;
		}

		public static void SaveToPrizeFile(this List<PrizeModel> models)
		{
			List<string> lines = new List<string>();

			models.ForEach(p =>
				lines.Add($"{ p.Id },{ p.PlaceNumber },{ p.PlaceName },{ p.PrizeAmount },{ p.PrizePercentage }")
			);

			File.WriteAllLines(GlobalConfig.PrizesFile.FullFilePath(), lines);
		}

		public static void SaveToPeopleFile(this List<PersonModel> models)
		{
			List<string> lines = new List<string>();

			models.ForEach(p =>
			{
				lines.Add($"{ p.Id },{ p.FirstName },{ p.LastName },{ p.Email },{ p.CellphoneNumber }");
			});

			File.WriteAllLines(GlobalConfig.PeopleFile.FullFilePath(), lines);
		}

		public static void SaveToTeamFile(this List<TeamModel> models)
		{
			List<string> lines = new List<string>();

			models.ForEach(t =>
			{
				lines.Add($"{ t.Id },{ t.TeamName },{ ConvertPeopleListToString(t.TeamMembers) }");
			});

			File.WriteAllLines(GlobalConfig.TeamFile.FullFilePath(), lines);
		}

		public static void SaveRoundsToFile(this TournamentModel model)
		{
			// Loop through each round
			// loop through each matchup
			// get the id for the new matchup and save the record
			// loop through each entry, get the id and save it

			model.Rounds.ForEach(round =>
			{
				round.ForEach(matchup =>
				{
					// Load all of the matchups from file
					// Get the top id and add one
					// Store the id
					// Save the matchup record

					matchup.SaveMatchupToFile();
				});
			});
		}

		private static List<MatchupEntryModel> ConvertStringToMatchupEntryModels(string input)
		{
			string[] ids = input.Split('|');
			List<MatchupEntryModel> output = new List<MatchupEntryModel>();
			List<string> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile();
			List<string> matchingEntries = new List<string>();

			ids.ToList().ForEach(id =>
			{
				entries.ForEach(entry =>
				{
					string[] cols = entry.Split(',');

					if (cols[0] == id)
					{
						matchingEntries.Add(entry);
					}
				});

				output = matchingEntries.ConvertToMatchupEntryModels();
			});

			return output;
		}

		private static TeamModel LookupTeamById(int id)
		{
			List<string> teams = GlobalConfig.TeamFile.FullFilePath().LoadFile();
			List<string> matchingTeams = new List<string>();

			teams.ForEach(team =>
			{
				string[] cols = team.Split(',');
				if (cols[0] == id.ToString())
				{
					matchingTeams.Add(team);
				}
			});

			return (matchingTeams.Count > 0) 
				? matchingTeams.ConvertToTeamModels().First()
				: null;
		}

		private static MatchupModel LookupMatchupById(int id)
		{
			List<string> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile();
			List<string> matchingMatchups = new List<string>();

			matchups.ForEach(matchup =>
			{
				string[] cols = matchup.Split(',');
				if (cols[0] == id.ToString())
				{
					matchingMatchups.Add(matchup);
				}
			});

			return (matchingMatchups.Count > 0)
				? matchingMatchups.ConvertToMatchupModels().First()
				: null;
		}

		public static List<MatchupEntryModel> ConvertToMatchupEntryModels(this List<string> lines)
		{
			List<MatchupEntryModel> output = new List<MatchupEntryModel>();
			lines.ForEach(line =>
			{
				string[] cols = line.Split(',');
				MatchupEntryModel me = new MatchupEntryModel();
				me.Id = int.Parse(cols[0]);
				if (cols[1].Length == 0)
				{
					me.TeamCompeting = null;
				}
				else
				{
					me.TeamCompeting = LookupTeamById(int.Parse(cols[1]));
				}
				me.Score = double.Parse(cols[2]);
				int parentId = 0;
				if (int.TryParse(cols[3], out parentId)) {
					me.ParentMatchup = LookupMatchupById(int.Parse(cols[3]));
				} 
				else
				{
					me.ParentMatchup = null;
				}
				output.Add(me);
			});

			return output;
		}

		public static List<MatchupModel> ConvertToMatchupModels(this List<string> lines)
		{
			List<MatchupModel> output = new List<MatchupModel>();
			lines.ForEach(line =>
			{
				string[] cols = line.Split(',');
				MatchupModel p = new MatchupModel();
				p.Id = int.Parse(cols[0]);
				p.Entries = ConvertStringToMatchupEntryModels(cols[1]);
				if (cols[2].Length == 0)
				{
					p.Winner = null;
				}
				else
				{
					p.Winner = LookupTeamById(int.Parse(cols[2]));
				}
				p.MatchupRound = int.Parse(cols[3]);
				output.Add(p);
			});
			return output;
		}

		public static void SaveMatchupToFile(this MatchupModel matchup)
		{

			List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

			int currentId = (matchups.Count > 0)
				? currentId = matchups.OrderByDescending(x => x.Id).First().Id + 1
				: 1;

			matchup.Id = currentId;

			matchups.Add(matchup);

			matchup.Entries.ForEach(entry =>
			{
				entry.SaveEntryToFile();
			});

			//save to file
			List<string> lines = new List<string>();

			matchups.ForEach(m =>
			{
				string winner = "";
				if (m.Winner != null)
				{
					winner = m.Winner.Id.ToString();
				}
				lines.Add($"{ m.Id },{ ConvertMatchupEntryListToString(m.Entries) },{ winner },{ m.MatchupRound }");
			});

			File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
		}

		public static void UpdateMatchupToFile(this MatchupModel matchup)
		{

			List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

			matchups.Remove(matchups.Where(x => x.Id == matchup.Id).First());

			matchups.Add(matchup);

			matchup.Entries.ForEach(entry =>
			{
				entry.UpdateEntryToFile();
			});

			//save to file
			List<string> lines = new List<string>();

			matchups.ForEach(m =>
			{
				string winner = "";
				if (m.Winner != null)
				{
					winner = m.Winner.Id.ToString();
				}
				lines.Add($"{ m.Id },{ ConvertMatchupEntryListToString(m.Entries) },{ winner },{ m.MatchupRound }");
			});

			File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
		}

		public static void SaveEntryToFile(this MatchupEntryModel entry)
		{
			List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();
			
			int currentId = (entries.Count > 0)
				? currentId = entries.OrderByDescending(x => x.Id).First().Id + 1
				: 1;

			entry.Id = currentId;
			entries.Add(entry);

			List<string> lines = new List<string>();

			entries.ForEach(e =>
			{
				string parent = "";
				if (e.ParentMatchup != null)
				{
					parent = e.ParentMatchup.Id.ToString();
				}
				string teamCompeting = "";
				if (e.TeamCompeting != null)
				{
					teamCompeting = e.TeamCompeting.Id.ToString();
				}
				lines.Add($"{ e.Id },{ teamCompeting },{ e.Score },{ parent }");
			});

			File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
		}

		public static void UpdateEntryToFile(this MatchupEntryModel entry)
		{
			List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

			entries.Remove(entries.Where(x => x.Id == entry.Id).First());

			entries.Add(entry);

			List<string> lines = new List<string>();

			entries.ForEach(e =>
			{
				string parent = "";
				if (e.ParentMatchup != null)
				{
					parent = e.ParentMatchup.Id.ToString();
				}
				string teamCompeting = "";
				if (e.TeamCompeting != null)
				{
					teamCompeting = e.TeamCompeting.Id.ToString();
				}
				lines.Add($"{ e.Id },{ teamCompeting },{ e.Score },{ parent }");
			});

			File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
		}

		public static void SaveToTournamentFile(this List<TournamentModel> models)
		{
			List<string> lines = new List<string>();
			models.ForEach(tm =>
			{
				lines.Add($"{ tm.Id },{ tm.TournamentName },{ tm.EntryFee },{ ConvertTeamListToString(tm.EnteredTeams) },{ ConvertPrizeListToString(tm.Prizes) },{ ConvertRoundListToString(tm.Rounds) }");
			});
			File.WriteAllLines(GlobalConfig.TournamentFile.FullFilePath(), lines);
		}

		private static string ConvertRoundListToString(List<List<MatchupModel>> rounds)
		{
			string output = "";
			if (rounds.Count == 0) { return output; }
			rounds.ForEach(r =>
			{
				output += $"{ ConvertMatchupListToString(r) }|";
			});
			output = output.Substring(0, output.Length - 1);
			return output;
		}

		private static string ConvertMatchupListToString(List<MatchupModel> matchups)
		{
			string output = "";
			if (matchups.Count == 0) { return output; }
			matchups.ForEach(m =>
			{
				output += $"{ m.Id }^";
			});
			output = output.Substring(0, output.Length - 1);
			return output;
		}

		private static string ConvertMatchupEntryListToString(List<MatchupEntryModel> entries)
		{
			string output = "";
			if (entries.Count == 0) { return output; }
			entries.ForEach(e =>
			{
				output += $"{ e.Id }|";
			});
			output = output.Substring(0, output.Length - 1);
			return output;
		}

		private static string ConvertPrizeListToString(List<PrizeModel> prizes)
		{
			string output = "";
			if (prizes.Count == 0) { return output; }
			prizes.ForEach(p =>
			{
				output += $"{ p.Id }|";
			});
			output = output.Substring(0, output.Length - 1);
			return output;
		}

		private static string ConvertTeamListToString(List<TeamModel> teams)
		{
			string output = "";

			if (teams.Count == 0) { return output; }

			teams.ForEach(t =>
			{
				output += $"{ t.Id }|";
			});

			output = output.Substring(0, output.Length - 1);

			return output;
		}

		private static string ConvertPeopleListToString(List<PersonModel> people)
		{
			string output = "";

			if (people.Count == 0) { return output; }

			people.ForEach(p =>
			{
				output += $"{ p.Id }|";
			});

			output = output.Substring(0, output.Length - 1);

			return output;
		}
	}
}
