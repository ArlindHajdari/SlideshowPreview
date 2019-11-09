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
            int fileToRead = 2;
            int numberOfFailedAttempts = 100;

            Random random = new Random();
            Solution solution = new Solution();

            string[] files = Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\Samples", "*.txt");

            List<Slide> slides = new List<Slide>();
            Instance instance = Extensions.IO.ReadInput(files[fileToRead]);

            Console.WriteLine($"Number of photos: {instance.NumberOfPhotos}\n");

            solution.GenerateRandomSolution(instance.Photos);
            solution.InterestFactor = solution.CalculateInterestFactor(solution.Slides.OrderBy(x => random.Next()).ToList());
            solution.HillClimbing(numberOfFailedAttempts);

            Console.WriteLine($"Number of slides: { solution.Slides.Count() }\n");
            Console.WriteLine($"Interest Factor: { solution.InterestFactor }");
            Console.ReadKey();
        }
    }
}
