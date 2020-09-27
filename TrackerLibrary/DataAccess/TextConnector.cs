using System;
using System.Collections.Generic;
using System.Text;
using TrackerLibrary.Models;
using TrackerLibrary.DataAccess.TextHelpers;
using System.Linq;

namespace TrackerLibrary.DataAccess
{
	public class TextConnector : IDataConnection
	{
		public void CompleteTournament(TournamentModel model)
		{
			List<TournamentModel> tournaments = GlobalConfig.TournamentFile
				.FullFilePath()
				.LoadFile()
				.ConvertToTournamentModels();

			tournaments.Remove(model);
			tournaments.SaveToTournamentFile();
			TournamentLogic.UpdateTournamentResults(model);
		}

		public void CreatePerson(PersonModel model)
		{
			List<PersonModel> people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();

			int currentId = (people.Count > 0)
				? currentId = people.OrderByDescending(x => x.Id).First().Id + 1
				: 1;

			model.Id = currentId;

			people.Add(model);

			people.SaveToPeopleFile();
		}

		/// <summary>
		/// Saves a new prize to the text file
		/// </summary>
		/// <param name="model">The prize informations</param>
		/// <returns>The prize information, including the unique identifier.</returns>
		public void CreatePrize(PrizeModel model)
		{
			// Load text file
			// Convert the text to List<prizemodel>
			List<PrizeModel> prizes = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();

			// Find the ID
			int currentId = (prizes.Count > 0) 
				? currentId = prizes.OrderByDescending(x => x.Id).First().Id + 1 
				: 1;

			model.Id = currentId;

			// Add the new record with new ID
			prizes.Add(model);

			// Convert the prizes to list<string>
			// Save the list<string> to the text file
			prizes.SaveToPrizeFile();
		}

		public void CreateTeam(TeamModel model)
		{
			List<TeamModel> teams = GlobalConfig.TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();

			int currentId = (teams.Count > 0)
				? currentId = teams.OrderByDescending(x => x.Id).First().Id + 1
				: 1;

			model.Id = currentId;

			teams.Add(model);

			teams.SaveToTeamFile();
		}

		public void CreateTournament(TournamentModel model)
		{
			List<TournamentModel> tournaments = GlobalConfig.TournamentFile
				.FullFilePath()
				.LoadFile()
				.ConvertToTournamentModels();

			int currentId = (tournaments.Count > 0)
				? currentId = tournaments.OrderByDescending(x => x.Id).First().Id + 1
				: 1;

			model.Id = currentId;

			model.SaveRoundsToFile();

			tournaments.Add(model);

			tournaments.SaveToTournamentFile();

			TournamentLogic.UpdateTournamentResults(model);
		}

		public List<PersonModel> GetPerson_All()
		{
			return GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();
		}

		public List<TeamModel> GetTeams_All()
		{
			return GlobalConfig.TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();
		}

		public List<TournamentModel> GetTournament_All()
		{
			return GlobalConfig.TournamentFile
				.FullFilePath()
				.LoadFile()
				.ConvertToTournamentModels();
		}

		public void UpdateMatchup(MatchupModel model)
		{
			model.UpdateMatchupToFile();
		}
	}
}
