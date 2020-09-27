using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using TrackerLibrary.Models;

//@PlaceNumber int,
//@PlaceName nvarchar(50),
//@PrizeAmount money,
//@PrizePercentage float,
//@id int=0 output

namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string db = "Tournaments";
        public void CreatePerson(PersonModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
                p.Add("@FirstName", model.FirstName);
                p.Add("@LastName", model.LastName);
                p.Add("@Email", model.Email);
                p.Add("@CellphoneNumber", model.CellphoneNumber);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPeople_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");
            }
        }

        /// <summary>
        /// Saves a new prize to the database
        /// </summary>
        /// <param name="model">The prize informations</param>
        /// <returns>The prize information, including the unique identifier.</returns>
        public void CreatePrize(PrizeModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
                p.Add("@PlaceNumber", model.PlaceNumber);
                p.Add("@PlaceName", model.PlaceName);
                p.Add("@PrizeAmount", model.PrizeAmount);
                p.Add("@PrizePercentage", model.PrizePercentage);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPrizes_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");
            }
        }

        public void CreateTeam(TeamModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
                p.Add("@TeamName", model.TeamName);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTeams_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                model.TeamMembers.ForEach(tm =>
                {
                    p = new DynamicParameters();
                    p.Add("@TeamId", model.Id);
                    p.Add("@PersonId", tm.Id);

                    connection.Execute("dbo.spTeamMembers_Insert", p, commandType: CommandType.StoredProcedure);
                });
            }
        }

		public void CreateTournament(TournamentModel model)
		{
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                SaveTournament(connection, model);

                SaveTournamentPrizes(connection, model);

                SaveTournamentEntries(connection, model);

                SaveTournamentRounds(connection, model);
			}
        }

        private void SaveTournamentPrizes(IDbConnection connection, TournamentModel model)
		{
            model.Prizes.ForEach(pz =>
            {
                var p = new DynamicParameters();
                p.Add("@TournamentId", model.Id);
                p.Add("@PrizeId", pz.Id);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                connection.Execute("dbo.spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);
            });
        }

        private void SaveTournamentEntries(IDbConnection connection, TournamentModel model)
		{
            model.EnteredTeams.ForEach(t =>
            {
                var p = new DynamicParameters();
                p.Add("@TournamentId", model.Id);
                p.Add("@TeamId", t.Id);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentEntries_Insert", p, commandType: CommandType.StoredProcedure);
            });
        }

        private void SaveTournamentRounds(IDbConnection connection, TournamentModel model)
		{
            // List<List<MatchupModel>> Rounds
            // List<MatchupEntries> Entries

            // Loop through the rounds
            // Loop through the matchups
            // Save the matchup
            // Loop through the entries and save them

            model.Rounds.ForEach(round =>
            {
                round.ForEach(matchup =>
                {
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", model.Id);
                    p.Add("@MatchupRound", matchup.MatchupRound);
                    p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    
                    connection.Execute("dbo.spMatchups_Insert", p, commandType: CommandType.StoredProcedure);
                    
                    matchup.Id = p.Get<int>("@id");

                    matchup.Entries.ForEach(entry =>
                    {
						p = new DynamicParameters();
						p.Add("@MatchupId", matchup.Id);

                        if (entry.ParentMatchup == null)
                        {
                            p.Add("@ParentMatchupId", null);
                        }
                        else
                        {
                            p.Add("@ParentMatchupId", entry.ParentMatchup.Id);
                        }

                        if (entry.TeamCompeting == null)
						{
							p.Add("@TeamCompetingid", null);
						} 
                        else
						{
                            p.Add("@TeamCompetingid", entry.TeamCompeting.Id);
                        }

                        p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

						connection.Execute("dbo.spMatchupEntries_Insert", p, commandType: CommandType.StoredProcedure);
					
                    
                    });
                });
            });
		}

        private void SaveTournament(IDbConnection connection, TournamentModel model)
		{
            var p = new DynamicParameters();
            p.Add("@TournamentName", model.TournamentName);
            p.Add("@EntryFee", model.EntryFee);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spTournaments_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");
        }

		public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }

            return output;
        }

        public List<TeamModel> GetTeams_All()
        {
            List<TeamModel> output;

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<TeamModel>("dbo.spTeams_GetAll").ToList();

                output.ForEach(team =>
                {
                    var p = new DynamicParameters();
                    p.Add("@TeamId", team.Id);
                    team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                });
            }

            return output;
        }

		public List<TournamentModel> GetTournament_All()
		{
            List<TournamentModel> output;
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<TournamentModel>("dbo.spTournaments_GetAll").ToList();
                output.ForEach(t =>
                {
					var p = new DynamicParameters();
					p.Add("@TournamentId", t.Id);

					// Populate prizes
					t.Prizes = connection.Query<PrizeModel>("dbo.spPrizes_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();

					// Populate teams
					t.EnteredTeams = connection.Query<TeamModel>("dbo.spTeams_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();
					t.EnteredTeams.ForEach(team =>
				    {
						var p2 = new DynamicParameters();
						p2.Add("@TeamId", team.Id);
						team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p2, commandType: CommandType.StoredProcedure).ToList();
					});

					// Populate rounds
					List<MatchupModel> matchups = connection.Query<MatchupModel>("dbo.spMatchups_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();

                    matchups.ForEach(m =>
                    {
                        p = new DynamicParameters();
                        p.Add("@MatchupId", m.Id);

                        m.Entries = connection.Query<MatchupEntryModel>("dbo.spMatchupEntries_GetByMatchup", p, commandType: CommandType.StoredProcedure).ToList();

                        if (m.WinnerId > 0)
						{
                            m.Winner = t.EnteredTeams.Where(x => x.Id == m.WinnerId).First();
						}

                        m.Entries.ForEach(me =>
                        {
                            if (me.TeamCompetingId > 0)
							{
                                me.TeamCompeting = t.EnteredTeams.Where(x => x.Id == me.TeamCompetingId).First();
							}

                            if (me.ParentMatchupId > 0)
							{
                                me.ParentMatchup = matchups.Where(x => x.Id == me.ParentMatchupId).First();
							}
                        });
                    });

                    List<MatchupModel> currRow = new List<MatchupModel>();
                    int currRound = 1;
                    matchups.ForEach(m =>
                    {
                        if (m.MatchupRound > currRound)
                        {
                            t.Rounds.Add(currRow);
                            currRow = new List<MatchupModel>();
                            currRound += 1;
                        }

                        currRow.Add(m);
                    });

                    t.Rounds.Add(currRow);
                });

			}
			return output;
        }

		public void UpdateMatchup(MatchupModel model)
		{
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
				if (model.Winner != null)
				{
					p.Add("@Id", model.Id);
					p.Add("@WinnerId", model.Winner.Id);

					connection.Execute("dbo.spMatchups_Update", p, commandType: CommandType.StoredProcedure); 
				}

                model.Entries.ForEach(me =>
                {
					if (me.TeamCompeting != null)
					{
						p = new DynamicParameters();
						p.Add("@Id", me.Id);
						p.Add("@TeamCompetingId", me.TeamCompeting.Id);
						p.Add("@Score", me.Score);
						connection.Execute("dbo.spMatchupEntries_Update", p, commandType: CommandType.StoredProcedure); 
					}
                });
            }
        }
	}
}
