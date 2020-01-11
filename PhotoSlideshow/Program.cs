using PhotoSlideshow.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace PhotoSlideshow
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Initializing values
            int fileToRead = 2;
            int timeToRun = 5;
            //int numberOfIterations  = 500;

            double temperature = 200;
            double alpha = 0.999;
            double epsilon = 0.00001;

            Random random = new Random();
            Solution solution = new Solution();
            Stopwatch stopwatch = new Stopwatch();

            string[] files = Directory.GetFiles($"Samples", "*.txt");
            Instance instance = Extensions.IO.ReadInput(files[fileToRead]);

            Console.WriteLine($"Number of photos: {instance.NumberOfPhotos}\n");
            #endregion

            #region Algorithm
            stopwatch.Start();

            solution.GenerateSolutionWithHeuristic(instance.Photos.OrderBy(x => x.Orientation).ThenBy(x => random.Next()).ToList(), stopwatch, timeToRun, 1000, 1);
            solution.FirstSolutionInterestFactor = solution.CalculateInterestFactor(solution.FirstSolutionSlides);

            solution.SecondSolutionSlides = new List<Slide>(solution.FirstSolutionSlides.OrderBy(x => random.Next()));
            solution.SecondSolutionInterestFactor = solution.CalculateInterestFactor(solution.SecondSolutionSlides);

            //solution.HillClimbing(numberOfIterations, stopwatch, timeToRun);
            solution.SimulatedAnnealing(temperature, alpha, epsilon, stopwatch, timeToRun);

            stopwatch.Stop();
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
