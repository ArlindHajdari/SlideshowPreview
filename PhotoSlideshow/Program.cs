using PhotoSlideshow.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace PhotoSlideshow
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Initializing
            int fileToRead = 3;
            int numberOfIterations = 150000;

            Random random = new Random();
            Solution solution = new Solution();

            string[] files = Directory.GetFiles($"Samples", "*.txt");

            List<Slide> slides = new List<Slide>();
            Instance instance = Extensions.IO.ReadInput(files[fileToRead]);

            Console.WriteLine($"Number of photos: {instance.NumberOfPhotos}\n");
            #endregion

            #region Algorithms in their pure form
            //solution.GenerateSolutionWithHeuristic(instance.Photos.OrderBy(x => x.Orientation).ThenBy(x => random.Next()).ToList(), 3000);
            //solution.InterestFactor = solution.CalculateInterestFactor(solution.Slides);
            //solution.HillClimbing(numberOfIterations);
            //solution.SimulatedAnnealing(400, 0.99, 0.00001);
            #endregion

            #region Algorithms with additional features
            solution.GenerateSolutionWithHeuristic(instance.Photos.OrderBy(x => x.Orientation).ThenBy(x => random.Next()).ToList(), 3000, 1);
            solution.FirstSolutionInterestFactor = solution.CalculateInterestFactor(solution.FirstSolutionSlides);

            solution.SecondSolutionSlides = new List<Slide>(solution.FirstSolutionSlides.OrderBy(x => random.Next()));
            solution.SecondSolutionInterestFactor = solution.CalculateInterestFactor(solution.SecondSolutionSlides);

            solution.HillClimbingWithAdditionalFeatures(numberOfIterations);
            //solution.SimulatedAnnealingWithAdditionalFeatures();
            #endregion

            #region Outputs
            solution.GenerateOutputFile($"{Path.GetFileNameWithoutExtension(files[fileToRead])}_result_{DateTime.Now.Ticks}.txt");

            Console.WriteLine($"Number of slides: { solution.Slides.Count() }\n");
            Console.WriteLine($"Interest Factor: { solution.InterestFactor }\n");

            Console.ReadKey();
            #endregion
        }
    }
}
