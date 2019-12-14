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
            int fileToRead = 2;
            int numberOfIterations = 500;

            Random random = new Random();
            Solution solution = new Solution();

            string[] files = Directory.GetFiles($"Samples", "*.txt");

            List<Slide> slides = new List<Slide>();
            Instance instance = Extensions.IO.ReadInput(files[fileToRead]);

            Console.WriteLine($"Number of photos: {instance.NumberOfPhotos}\n");
            #endregion

            #region Algorithms in their pure form
            //solution.GenerateRandomSolution(instance.Photos.OrderBy(x => random.Next()).ToList());
            //solution.InterestFactor = solution.CalculateInterestFactor(solution.Slides);
            //solution.HillClimbing(numberOfIterations);
            //solution.SimulatedAnnealing();
            #endregion

            #region Algorithms with additional features
            solution.GenerateRandomSolution(instance.Photos.OrderBy(x => random.Next()).ToList(), 1);
            solution.FirstSolutionInterestFactor = solution.CalculateInterestFactor(solution.FirstSolutionSlides);

            solution.GenerateRandomSolution(instance.Photos.OrderBy(x => random.Next()).ToList(), 2);
            solution.SecondSolutionInterestFactor = solution.CalculateInterestFactor(solution.SecondSolutionSlides);

            //solution.HillClimbingWithAdditionalFeatures(numberOfIterations);
            solution.SimulatedAnnealingWithAdditionalFeatures();
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
