using PhotoSlideshow.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            int photosCount = photos.Count();

            while (photosToSkip.Count() < photosCount)
            {
                int randomStart = random.Next(0, photosCount - 1);
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
                                x.Tags.Where(t => photo.Tags.Contains(t)).Count())
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
        public (int, int) Mutate(List<Slide> slides, List<int> randomNumbers)
        {
            (int, int) response = (0, 0);
            Random random = new Random();

            int mutationSelector = random.Next(0, 10);
            List<int> slidesToSwap = slides.Where(x => x.Photos.Count == 2).OrderBy(x => random.Next()).Select(x => x.Id).Take(2).ToList();

            if (mutationSelector < 3 && slidesToSwap.Count == 2)
            {
                int firstSlidePhotoIndex = random.Next(0, 2);
                int secondSlidePhotoIndex = random.Next(0, 2);

                int firstSlideIndex = slides.IndexOf(slides.FirstOrDefault(x => x.Id == slidesToSwap.FirstOrDefault()));
                int secondSlideIndex = slides.IndexOf(slides.FirstOrDefault(x => x.Id == slidesToSwap.LastOrDefault()));

                response.Item1 = MutationInterestFactor(slides, firstSlideIndex, secondSlideIndex, 1);

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

                response.Item2 = MutationInterestFactor(slides, firstSlideIndex, secondSlideIndex, 1);
            }
            else if (mutationSelector < 7)
            {
                slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();

                response.Item1 = MutationInterestFactor(slides, slidesToSwap.FirstOrDefault(), slidesToSwap.LastOrDefault(), 1);

                Slide tempSlide = slides[slidesToSwap.FirstOrDefault()];
                slides[slidesToSwap.FirstOrDefault()] = slides[slidesToSwap.LastOrDefault()];
                slides[slidesToSwap.LastOrDefault()] = tempSlide;

                response.Item2 = MutationInterestFactor(slides, slidesToSwap.FirstOrDefault(), slidesToSwap.LastOrDefault(), 1);
            }
            else if (mutationSelector < 9)
            {
                slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();

                response.Item1 = MutationInterestFactor(slides, slidesToSwap.OrderBy(x => x).FirstOrDefault(), slidesToSwap.OrderBy(x => x).LastOrDefault(), 2);

                Slide slide = slides[slidesToSwap.FirstOrDefault()];
                slides.RemoveAt(slidesToSwap.FirstOrDefault());
                slides.Insert(slidesToSwap.LastOrDefault(), slide);

                response.Item2 = MutationInterestFactor(slides, slidesToSwap.OrderBy(x => x).FirstOrDefault(), slidesToSwap.OrderBy(x => x).LastOrDefault(), 2);
            }
            else
            {
                int slidesCount = slides.Count();
                int skip = random.Next(0, slidesCount);
                int take = random.Next(0, slidesCount - skip);

                response.Item1 = MutationInterestFactor(slides, skip, skip + take, 2);

                slides.Skip(skip).Take(take).OrderBy(x => random.Next()).ToList();

                response.Item2 = MutationInterestFactor(slides, skip, skip + take, 2);
            }
            return response;
        }

        public void CheckHCForImprovements(List<Slide> firstSolution, List<Slide> secondSolution)
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

        public void CheckSAForImprovements(List<Slide> firstSolution, List<Slide> secondSolution, int firstInterestFactor, int secondInterestFactor, double temperature)
        {
            if (TakeSolution(this.FirstSolutionInterestFactor, firstInterestFactor, temperature))
            {
                this.FirstSolutionSlides = firstSolution;
                this.FirstSolutionInterestFactor = firstInterestFactor;
            }

            if (TakeSolution(this.SecondSolutionInterestFactor, secondInterestFactor, temperature))
            {
                this.SecondSolutionSlides = secondSolution;
                this.SecondSolutionInterestFactor = secondInterestFactor;
            }
        }

        public bool TakeSolution(int qualityOfS, int qualityOfR, double temperature)
        {
            Random random = new Random();
            double quality = qualityOfR - qualityOfS;

            double e = Math.Pow(Math.E, quality / temperature);
            double nextDouble = random.NextDouble();

            return qualityOfR > qualityOfS || nextDouble < e / 2;
        }

        public void SetBestSolution()
        {
            if (this.FirstSolutionInterestFactor > this.InterestFactor || this.SecondSolutionInterestFactor > this.InterestFactor)
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

            SetBestSolution();

            for (int i = 0; i < numberOfIterations; i++)
            {
                List<Slide> firstTempSolution = DeepCopyFirstSlides();
                List<Slide> secondTempSolution = DeepCopySecondSlides();

                Mutate(firstTempSolution, randomNumbers);
                Mutate(secondTempSolution, randomNumbers);

                CheckHCForImprovements(firstTempSolution, secondTempSolution);
            }

            SetBestSolution();
        }

        //lundy & mees
        public void SimulatedAnnealing(double temperature, double alpha, double epsilon, int timeToRun)
        {
            Stopwatch stopwatch = new Stopwatch();
            List<int> randomNumbers = new List<int>();

            int slideNumber = this.FirstSolutionSlides.Count();

            for (int i = 0; i < slideNumber; i++)
            {
                randomNumbers.Add(i);
            }

            SetBestSolution();

            stopwatch.Start();

            while (temperature > epsilon && stopwatch.Elapsed.TotalMinutes < timeToRun)
            {
                List<Slide> firstTempSolution = DeepCopyFirstSlides();
                List<Slide> secondTempSolution = DeepCopySecondSlides();

                Console.WriteLine($"Current temperature: { temperature }");

                (int, int) firstSlideMutation = Mutate(firstTempSolution, randomNumbers);
                int firstInterestFactor = this.FirstSolutionInterestFactor - firstSlideMutation.Item1 + firstSlideMutation.Item2;

                (int, int) secondSlideMutation = Mutate(secondTempSolution, randomNumbers);
                int secondInterestFactor = this.SecondSolutionInterestFactor - secondSlideMutation.Item1 + secondSlideMutation.Item2;

                CheckSAForImprovements(firstTempSolution, secondTempSolution, firstInterestFactor, secondInterestFactor, temperature);

                temperature *= alpha;

                SetBestSolution();
            }

            stopwatch.Stop();
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

        public int MutationInterestFactor(List<Slide> slides, int firstIndex, int secondIndex, int mode)
        {
            int interestFactor = 0;
            int slidesCount = Slides.Count() - 1;

            int difference = Math.Abs(firstIndex - secondIndex);

            if (mode == 1 && difference != 1)
            {
                interestFactor += CalculateInterestFactorForOneSlide(slides, firstIndex);
                interestFactor += CalculateInterestFactorForOneSlide(slides, secondIndex);
            }
            else
            {
                int start = firstIndex > secondIndex ? (secondIndex > 0 ? secondIndex - 1 : secondIndex) : (firstIndex > 0 ? firstIndex - 1 : firstIndex);
                int end = secondIndex < firstIndex ? (secondIndex > 0 ? firstIndex + 1 : secondIndex) : (secondIndex < slidesCount ? secondIndex + 1 : secondIndex);
                interestFactor = CalculateSlidesBetweenTwoPoints(slides, start, end);
            }

            return interestFactor;
        }

        public int CalculateSlidesBetweenTwoPoints(List<Slide> slides, int start, int end)
        {
            int interestFactor = 0;

            for (int i = start; i < end; i++)
            {
                int commonTags = CalculateCommonSlideTags(slides[i], slides[i + 1]);
                int slideAnotB = CalculateDifferenteSlideTags(slides[i], slides[i + 1]);
                int slideBnotA = CalculateDifferenteSlideTags(slides[i + 1], slides[i]);
                interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }

            return interestFactor;
        }

        public int CalculateInterestFactorForOneSlide(List<Slide> slides, int index)
        {
            int interestFactor = 0;
            if (index < slides.Count() - 1)
            {
                int commonTags = CalculateCommonSlideTags(slides[index], slides[index + 1]);
                int slideAnotB = CalculateDifferenteSlideTags(slides[index], slides[index + 1]);
                int slideBnotA = CalculateDifferenteSlideTags(slides[index + 1], slides[index]);
                interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));

                if (index > 0)
                {
                    commonTags = CalculateCommonSlideTags(slides[index - 1], slides[index]);
                    slideAnotB = CalculateDifferenteSlideTags(slides[index - 1], slides[index]);
                    slideBnotA = CalculateDifferenteSlideTags(slides[index], slides[index - 1]);
                    interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
                }
            }
            else
            {
                int commonTags = CalculateCommonSlideTags(slides[index - 1], slides[index]);
                int slideAnotB = CalculateDifferenteSlideTags(slides[index - 1], slides[index]);
                int slideBnotA = CalculateDifferenteSlideTags(slides[index], slides[index - 1]);
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
