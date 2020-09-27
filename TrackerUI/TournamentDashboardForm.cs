using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class TournamentDashboardForm : Form, ITournamentRequester
    {
        public TournamentDashboardForm()
        {
            InitializeComponent();

            WireUpLists();
        }

        private void WireUpLists()
		{
            loadExistingTournamentDropdown.DataSource = GlobalConfig.Connection.GetTournament_All();
            loadExistingTournamentDropdown.DisplayMember = "TournamentName";
		}

		private void createTournamentButton_Click(object sender, EventArgs e)
		{
            CreateTournamentForm frm = new CreateTournamentForm(this);
            frm.Show();
		}

		private void loadTournamentButton_Click(object sender, EventArgs e)
		{
            TournamentModel tm = (TournamentModel)loadExistingTournamentDropdown.SelectedItem;
            TournamentViewerForm frm = new TournamentViewerForm(tm);
            frm.Show();
		}

		public void TournamentComplete()
		{
            WireUpLists();
		}
	}
}
