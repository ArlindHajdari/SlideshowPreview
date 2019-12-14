﻿using PhotoSlideshow.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PhotoSlideshow.Models
{
    public class Solution
    {
        public List<Slide> Slides { get; set; }
        public int InterestFactor { get; set; } = int.MinValue;

        public List<Slide> FirstSolutionSlides { get; set; }
        public int FirstSolutionInterestFactor { get; set; } = int.MinValue;

        public List<Slide> SecondSolutionSlides { get; set; }
        public int SecondSolutionInterestFactor { get; set; } = int.MinValue;

        public Solution()
        {
            this.Slides = new List<Slide>();
            this.FirstSolutionSlides = new List<Slide>();
            this.SecondSolutionSlides = new List<Slide>();
        }

        public Solution(List<Slide> Slides)
        {
            this.Slides = Slides;
        }

        public List<Slide> DeepCopySlides()
        {
            List<Slide> slides = this.Slides.ConvertAll(x => new Slide(x.Id, x.Photos));
            return slides;
        }

        public List<Slide> DeepCopyFirstSlides()
        {
            List<Slide> slides = this.FirstSolutionSlides.ConvertAll(x => new Slide(x.Id, x.Photos));
            return slides;
        }

        public List<Slide> DeepCopySecondSlides()
        {
            List<Slide> slides = this.SecondSolutionSlides.ConvertAll(x => new Slide(x.Id, x.Photos));
            return slides;
        }

        public static T DeepClone<T>(T obj)
        {
            T objResult;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Position = 0;
                objResult = (T)bf.Deserialize(ms);
            }
            return objResult;
        }

        #region [Functions]

        public void GenerateRandomSolution(List<Photo> photos, int? firstOrSecond = null)
        {
            int slideId = 0;
            Random random = new Random();
            List<int> photosToSkip = new List<int>();

            while (photosToSkip.Count() < photos.Count())
            {
                Photo photo;
                if (photosToSkip.Count() == 0)
                {
                    int randomStart = random.Next(0, photos.Count() - 1);
                    photo = photos.Where(x => randomStart == x.Id).FirstOrDefault();
                }
                else
                {
                    photo = photos.Where(x => !photosToSkip.Contains(x.Id))
                       .OrderByDescending(x =>
                           x.Tags.Where(t => !this.Slides.LastOrDefault().Tags.Contains(t)).Count() +
                           x.Tags.Where(t => this.Slides.LastOrDefault().Tags.Contains(t)).Count() +
                           this.Slides.LastOrDefault().Tags.Where(t => x.Tags.Contains(t)).Count())
                       .FirstOrDefault();
                }

                List<Photo> photosToAdd = new List<Photo>()
                {
                    photo
                };

                if (photo.Orientation == Orientation.V)
                {
                    Photo secondPhoto = photos
                        .Where(x => x.Id != photo.Id && x.Orientation.Equals(Orientation.V) && !photosToSkip.Contains(x.Id))
                        .OrderByDescending(x =>
                            x.Tags.Where(t => !photo.Tags.Contains(t)).Count() +
                            x.Tags.Where(t => photo.Tags.Contains(t)).Count() +
                            photo.Tags.Where(t => x.Tags.Contains(t)).Count())
                        .FirstOrDefault();

                    if (secondPhoto != null)
                    {
                        photosToAdd.Add(secondPhoto);
                        photosToSkip.Add(secondPhoto.Id);
                    }
                }

                photosToSkip.Add(photo.Id);
                this.Slides.Add(new Slide(slideId, photosToAdd));
                slideId++;
            }
            if (firstOrSecond.HasValue)
            {
                if (firstOrSecond == 1)
                {
                    this.FirstSolutionSlides = new List<Slide>(this.Slides);
                    this.Slides = new List<Slide>();
                }
                else
                {
                    this.SecondSolutionSlides = new List<Slide>(this.Slides);
                    this.Slides = new List<Slide>();
                }
            }
        }

        public void SwapSlidesOrPhotos(List<Slide> slides, List<int> randomNumbers)
        {
            Random random = new Random();
            int swapOrChange = random.Next(0, 9);
            List<int> slidesToSwap = slides.Where(x => x.Photos.Count == 2).OrderBy(x => random.Next()).Select(x => x.Id).Take(2).ToList();

            if (swapOrChange < 5 && slidesToSwap.Count == 2)
            {
                int firstSlidePhotoIndex = random.Next(0, 2);
                int secondSlidePhotoIndex = random.Next(0, 2);

                int firstSlideIndex = slides.IndexOf(slides.FirstOrDefault(x => x.Id == slidesToSwap.FirstOrDefault()));
                int secondSlideIndex = slides.IndexOf(slides.FirstOrDefault(x => x.Id == slidesToSwap.LastOrDefault()));

                Slide slideA = DeepClone(slides[firstSlideIndex]);
                Slide slideB = DeepClone(slides[firstSlideIndex]);

                slideA.Photos[firstSlidePhotoIndex] = slides[secondSlideIndex].Photos[secondSlidePhotoIndex];
                slideB.Photos[secondSlidePhotoIndex] = slides[firstSlideIndex].Photos[firstSlidePhotoIndex];

                slides[firstSlideIndex] = slideA;
                slides[secondSlideIndex] = slideB;
            }
            else
            {
                slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();

                Slide tempSlide = slides[slidesToSwap.FirstOrDefault()];
                slides[slidesToSwap.FirstOrDefault()] = slides[slidesToSwap.LastOrDefault()];
                slides[slidesToSwap.LastOrDefault()] = tempSlide;
            }
        }

        public void CheckSolutionsForImprovements(List<Slide> firstSolution, List<Slide> secondSolution)
        {
            int firstSolutionInterestFactor = CalculateInterestFactor(firstSolution);
            if (firstSolutionInterestFactor >= this.FirstSolutionInterestFactor)
            {
                this.FirstSolutionSlides = firstSolution;
                this.FirstSolutionInterestFactor = firstSolutionInterestFactor;
            }

            int secondSolutionInterestFactor = CalculateInterestFactor(secondSolution);
            if (secondSolutionInterestFactor >= this.SecondSolutionInterestFactor)
            {
                this.SecondSolutionSlides = secondSolution;
                this.SecondSolutionInterestFactor = secondSolutionInterestFactor;
            }
        }

        public void SetBestSolution()
        {
            if (this.FirstSolutionInterestFactor > this.SecondSolutionInterestFactor)
            {
                this.Slides = this.FirstSolutionSlides;
                this.InterestFactor = this.FirstSolutionInterestFactor;
            }
            else
            {
                this.Slides = this.SecondSolutionSlides;
                this.InterestFactor = this.SecondSolutionInterestFactor;
            }
        }

        public void HillClimbing(int numberOfIterations)
        {
            Random random = new Random();
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < this.Slides.Count(); i++)
            {
                randomNumbers.Add(i);
            }

            for (int i = 0; i < numberOfIterations; i++)
            {
                List<Slide> tempSolution = DeepCopySlides();
                List<int> slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();

                Slide tempSlide = tempSolution[slidesToSwap.FirstOrDefault()];
                tempSolution[slidesToSwap.FirstOrDefault()] = tempSolution[slidesToSwap.LastOrDefault()];
                tempSolution[slidesToSwap.LastOrDefault()] = tempSlide;

                int currentInterestFactor = CalculateInterestFactor(tempSolution);
                if (currentInterestFactor >= this.InterestFactor)
                {
                    this.Slides = new List<Slide>(tempSolution);
                    this.InterestFactor = currentInterestFactor;
                }
            }
        }

        public void HillClimbingWithAdditionalFeatures(int numberOfIterations)
        {
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < this.Slides.Count(); i++)
            {
                randomNumbers.Add(i);
            }

            for (int i = 0; i < numberOfIterations; i++)
            {
                List<Slide> firstTempSolution = DeepCopyFirstSlides();
                List<Slide> secondTempSolution = DeepCopySecondSlides();

                SwapSlidesOrPhotos(firstTempSolution, randomNumbers);
                SwapSlidesOrPhotos(secondTempSolution, randomNumbers);

                CheckSolutionsForImprovements(firstTempSolution, secondTempSolution);
            }

            SetBestSolution();
        }

        public void SimulatedAnnealing(double temperature = 400.0, double alpha = 0.999, double epsilon = 0.001)
        {
            Random random = new Random();
            double maxTemperature = temperature;
            int slideNumber = this.Slides.Count();
            List<int> randomNumbers = new List<int>();

            for (int i = 0; i < slideNumber; i++)
            {
                randomNumbers.Add(i);
            }

            Console.WriteLine($"Initial Values\ntemperature: { temperature }, alpha: { alpha }, epsilon: {epsilon}.");

            while (temperature > epsilon)
            {
                temperature *= alpha;
                List<Slide> tempSolution = DeepCopySlides();
                int normalizedValue = (int)Math.Ceiling((temperature / maxTemperature) * slideNumber);

                for (int i = 0; i < normalizedValue; i++)
                {
                    List<int> slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();

                    Slide tempSlide = tempSolution[slidesToSwap.FirstOrDefault()];
                    tempSolution[slidesToSwap.FirstOrDefault()] = tempSolution[slidesToSwap.LastOrDefault()];
                    tempSolution[slidesToSwap.LastOrDefault()] = tempSlide;
                }

                int currentInterestFactor = CalculateInterestFactor(tempSolution);
                if (currentInterestFactor >= this.InterestFactor)
                {
                    this.Slides = new List<Slide>(tempSolution);
                    this.InterestFactor = currentInterestFactor;
                }
            }
        }

        public void SimulatedAnnealingWithAdditionalFeatures(double temperature = 400.0, double alpha = 0.999, double epsilon = 0.001)
        {
            double maxTemperature = temperature;
            int slideNumber = this.FirstSolutionSlides.Count();
            List<int> randomNumbers = new List<int>();

            for (int i = 0; i < slideNumber; i++)
            {
                randomNumbers.Add(i);
            }

            Console.WriteLine($"Initial Values\ntemperature: { temperature }, alpha: { alpha }, epsilon: {epsilon}.");

            while (temperature > epsilon)
            {
                temperature *= alpha;
                List<Slide> firstTempSolution = DeepCopyFirstSlides();
                List<Slide> secondTempSolution = DeepCopySecondSlides();

                int normalizedValue = (int)Math.Ceiling((temperature / maxTemperature) * slideNumber);
                Console.WriteLine($"Nomalized Value { normalizedValue }\t\tCurrent temperature: { temperature }");

                for (int i = 0; i < normalizedValue; i++)
                {
                    SwapSlidesOrPhotos(firstTempSolution, randomNumbers);
                    SwapSlidesOrPhotos(secondTempSolution, randomNumbers);
                }
                CheckSolutionsForImprovements(firstTempSolution, secondTempSolution);
            }
            SetBestSolution();
        }

        public int CalculateInterestFactor(List<Slide> slides)
        {
            int interestFactor = 0;
            for (int i = 0; i < slides.Count - 1; i++)
            {
                int commonTags = CalculateCommonSlideTags(slides[i], slides[i + 1]);
                int slideAnotB = CalculateDifferenteSlideTags(slides[i], slides[i + 1]);
                int slideBnotA = CalculateDifferenteSlideTags(slides[i + 1], slides[i]);
                interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }
            return interestFactor;
        }

        public int CalculateCommonSlideTags(Slide slideA, Slide slideB)
        {
            return slideA.Tags.Where(x => slideB.Tags.Contains(x)).Count();
        }

        public int CalculateDifferenteSlideTags(Slide slideA, Slide slideB)
        {
            return slideA.Tags.Where(x => !slideB.Tags.Contains(x)).Count();
        }

        public void GenerateOutputFile(string filename)
        {
            using (StreamWriter file = new StreamWriter(new FileStream(filename, FileMode.CreateNew)))
            {
                file.WriteLine(this.Slides.Count);
                foreach (Slide slide in this.Slides)
                {
                    file.WriteLine($"{string.Join(" ", slide.Photos.Select(x => x.Id).ToList())}");
                }
            }
        }

        #endregion
    }
}
