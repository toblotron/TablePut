using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TablePutter.Models;

namespace TablePutter.UserControls
{
    /// <summary>
    /// Interaction logic for RestaurantView.xaml
    /// </summary>
    public partial class RestaurantView : UserControl
    {
        public RestaurantView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ModelRoot.Instance.Problem.PlaceNextFromQueue();
            ModelRoot.Instance.Problem.UpdateGoalBookings();
        }

        private void PlaceAll_Click(object sender, RoutedEventArgs e)
        {
            while(ModelRoot.Instance.Problem.PlaceNextFromQueue());
            ModelRoot.Instance.Problem.UpdateGoalBookings();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // reset / init genetic solver
            ModelRoot.Instance.Problem.solver.Reset(Constants.populationSize, ModelRoot.Instance.Problem);
        }

        private void NextSolveButton_Click(object sender, RoutedEventArgs e)
        {
            // run next batch of generations
            ModelRoot.Instance.Problem.UpdateGoalBookings();
            ModelRoot.Instance.Problem.solver.NextBatch();
        }

        private void AdaptSolution_Click(object sender, RoutedEventArgs e)
        {
            // take the current top solution and make bookinassignments from it
            ModelRoot.Instance.Problem.AdaptAdequateSolution();
        }

        private void Unplace_Click(object sender, RoutedEventArgs e)
        {
            ModelRoot.Instance.Problem.RemovePreviousFromQueue();
            ModelRoot.Instance.Problem.UpdateGoalBookings();
        }

        private void SkipNext_Click(object sender, RoutedEventArgs e)
        {
            ModelRoot.Instance.Problem.SkipNextBooking();
        }

        private void EvaluateTop_Click(object sender, RoutedEventArgs e)
        {
            ModelRoot.Instance.Problem.EvaluateLeader();
        }
    }
}
