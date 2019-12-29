using PhotoSlideshow.Extensions;
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
        #region Variables
        public List<Slide> Slides { get; set; }
        public int InterestFactor { get; set; } = int.MinValue;

        public List<Slide> FirstSolutionSlides { get; set; }
        public int FirstSolutionInterestFactor { get; set; } = int.MinValue;

        public List<Slide> SecondSolutionSlides { get; set; }
        public int SecondSolutionInterestFactor { get; set; } = int.MinValue;
        #endregion

        #region Contructors
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
        #endregion

        #region Copy/Clone objects
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
        #endregion

        #region Generate Solution
        public void GenerateRandomSolution(List<Photo> photos)
        {
            int slideId = 0;
            Random random = new Random();
            List<int> photosToSkip = new List<int>();

            while (photosToSkip.Count() < photos.Count())
            {
                int randomStart = random.Next(0, photos.Count() - 1);
                Photo photo = photos.Where(x => randomStart == x.Id).FirstOrDefault();

                List<Photo> photosToAdd = new List<Photo>()
                {
                    photo
                };

                if (photo.Orientation == Orientation.V)
                {
                    Photo secondPhoto = photos.FirstOrDefault(x => x.Id != photo.Id && x.Orientation.Equals(Orientation.V) && !photosToSkip.Contains(x.Id));
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
        }

        public void GenerateSolutionWithHeuristic(List<Photo> photos, int takePhotosNumber = 1000, int? firstOrSecond = null)
        {
            Random random = new Random();

            int slideId = 0;
            int photosCount = photos.Count();

            int normalizedValue = (int)Math.Ceiling((decimal)photosCount / takePhotosNumber);

            for (int i = 0; i < normalizedValue; i++)
            {
                List<Photo> tempPhotos = new List<Photo>(photos.Skip(i * takePhotosNumber).Take(takePhotosNumber));
                int tempPhotosCount = tempPhotos.Count();
                int iterationCount = 0;

                while (iterationCount < tempPhotosCount)
                {
                    Photo photo;
                    if (iterationCount != 0 && i != 0)
                    {
                        photo = tempPhotos.OrderByDescending(x =>
                                            x.Tags.Where(t => !this.Slides.LastOrDefault().Tags.Contains(t)).Count() +
                                            x.Tags.Where(t => this.Slides.LastOrDefault().Tags.Contains(t)).Count() +
                                            this.Slides.LastOrDefault().Tags.Where(t => x.Tags.Contains(t)).Count())
                                        .FirstOrDefault();
                    }
                    else
                    {
                        photo = tempPhotos.FirstOrDefault();
                    }


                    List<Photo> photosToAdd = new List<Photo>()
                    {
                        photo
                    };

                    if (photo.Orientation == Orientation.V)
                    {
                        Photo secondPhoto = tempPhotos
                            .Where(x => x.Id != photo.Id && x.Orientation.Equals(Orientation.V))
                            .OrderByDescending(x =>
                                x.Tags.Where(t => !photo.Tags.Contains(t)).Count() +
                                x.Tags.Where(t => photo.Tags.Contains(t)).Count() +
                                photo.Tags.Where(t => x.Tags.Contains(t)).Count())
                            .FirstOrDefault();

                        if (secondPhoto != null)
                        {
                            photosToAdd.Add(secondPhoto);
                            tempPhotos.Remove(secondPhoto);

                            iterationCount++;
                        }
                    }

                    this.Slides.Add(new Slide(slideId, photosToAdd));
                    tempPhotos.Remove(photo);

                    iterationCount++;
                    slideId++;
                }
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
        #endregion

        #region Functions
        public void Mutate(List<Slide> slides, List<int> randomNumbers)
        {
            Random random = new Random();
            int mutationSelector = random.Next(0, 10);
            List<int> slidesToSwap = slides.Where(x => x.Photos.Count == 2).OrderBy(x => random.Next()).Select(x => x.Id).Take(2).ToList();

            if (mutationSelector < 3 && slidesToSwap.Count == 2)
            {
                int firstSlidePhotoIndex = random.Next(0, 2);
                int secondSlidePhotoIndex = random.Next(0, 2);

                int firstSlideIndex = slides.IndexOf(slides.FirstOrDefault(x => x.Id == slidesToSwap.FirstOrDefault()));
                int secondSlideIndex = slides.IndexOf(slides.FirstOrDefault(x => x.Id == slidesToSwap.LastOrDefault()));

                List<Photo> firstSlidePhotos = new List<Photo>
                {
                    new Photo(slides[firstSlideIndex].Photos.FirstOrDefault().Id, Orientation.V, new List<string>(slides[firstSlideIndex].Photos.FirstOrDefault().Tags)),
                    new Photo(slides[firstSlideIndex].Photos.LastOrDefault().Id, Orientation.V, new List<string>(slides[firstSlideIndex].Photos.LastOrDefault().Tags))
                };

                List<Photo> secondSlidePhotos = new List<Photo>
                {
                    new Photo(slides[secondSlideIndex].Photos.FirstOrDefault().Id, Orientation.V, new List<string>(slides[secondSlideIndex].Photos.FirstOrDefault().Tags)),
                    new Photo(slides[secondSlideIndex].Photos.LastOrDefault().Id, Orientation.V, new List<string>(slides[secondSlideIndex].Photos.LastOrDefault().Tags))
                };

                Slide slideA = new Slide(slides[firstSlideIndex].Id, firstSlidePhotos);
                Slide slideB = new Slide(slides[secondSlideIndex].Id, secondSlidePhotos);

                slideA.Photos[firstSlidePhotoIndex] = slides[secondSlideIndex].Photos[secondSlidePhotoIndex];
                slideB.Photos[secondSlidePhotoIndex] = slides[firstSlideIndex].Photos[firstSlidePhotoIndex];

                slides[firstSlideIndex] = slideA;
                slides[secondSlideIndex] = slideB;
            }
            else if (mutationSelector < 7)
            {
                slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();

                Slide tempSlide = slides[slidesToSwap.FirstOrDefault()];
                slides[slidesToSwap.FirstOrDefault()] = slides[slidesToSwap.LastOrDefault()];
                slides[slidesToSwap.LastOrDefault()] = tempSlide;
            }
            else if (mutationSelector < 9)
            {
                slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();
                Slide slide = slides[slidesToSwap.FirstOrDefault()];
                slides.RemoveAt(slidesToSwap.FirstOrDefault());
                slides.Insert(slidesToSwap.LastOrDefault(), slide);
            }
            else
            {
                int slidesCount = slides.Count();
                int skip = random.Next(0, slidesCount);
                int take = random.Next(0, slidesCount - skip);
                slides.Skip(skip).Take(take).OrderBy(x => random.Next()).ToList();
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

        #endregion

        #region Algorithms
        public void HillClimbing(int numberOfIterations)
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

                Mutate(firstTempSolution, randomNumbers);
                Mutate(secondTempSolution, randomNumbers);

                CheckSolutionsForImprovements(firstTempSolution, secondTempSolution);
            }

            SetBestSolution();
        }

        public void SimulatedAnnealing(double temperature, double alpha, double epsilon)
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

                int normalizedValue = (int)Math.Ceiling(((temperature / 4) / maxTemperature) * slideNumber);
                Console.WriteLine($"Nomalized Value { normalizedValue }\t\tCurrent temperature: { temperature }");

                for (int i = 0; i < normalizedValue; i++)
                {
                    Mutate(firstTempSolution, randomNumbers);
                    Mutate(secondTempSolution, randomNumbers);
                }
                CheckSolutionsForImprovements(firstTempSolution, secondTempSolution);
            }
            SetBestSolution();
        }
        #endregion

        #region Interest Factor
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
        #endregion

        #region Output file
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
