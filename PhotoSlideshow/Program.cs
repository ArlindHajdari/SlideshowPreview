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
            #region Initializing values
            int fileToRead = 2;
            int numberOfIterations = 500;

            double temperature = 400;
            double alpha = 0.999;
            double epsilon = 0.0001;

            Random random = new Random();
            Solution solution = new Solution();

            string[] files = Directory.GetFiles($"Samples", "*.txt");

            List<Slide> slides = new List<Slide>();
            Instance instance = Extensions.IO.ReadInput(files[fileToRead]);

            Console.WriteLine($"Number of photos: {instance.NumberOfPhotos}\n");
            #endregion

            #region Algorithm
            solution.GenerateSolutionWithHeuristic(instance.Photos.OrderBy(x => x.Orientation).ThenBy(x => random.Next()).ToList(), 1000, 1);
            solution.FirstSolutionInterestFactor = solution.CalculateInterestFactor(solution.FirstSolutionSlides);

            solution.SecondSolutionSlides = new List<Slide>(solution.FirstSolutionSlides.OrderBy(x => random.Next()));
            solution.SecondSolutionInterestFactor = solution.CalculateInterestFactor(solution.SecondSolutionSlides);

            //solution.HillClimbing(numberOfIterations);
            solution.SimulatedAnnealing(temperature, alpha, epsilon);
            #endregion

            #region Outputs
            solution.GenerateOutputFile($"{Path.GetFileNameWithoutExtension(files[fileToRead])}_result_{DateTime.Now.Ticks}.txt");

            Console.WriteLine($"Number of slides: { solution.Slides.Count() }\n");
            Console.WriteLine($"Interest Factor: { solution.InterestFactor }\n");
            Console.WriteLine($"Interest Factor: { solution.CalculateInterestFactor(solution.Slides) }\n");

            Console.ReadKey();
            #endregion
        }
    }
}
