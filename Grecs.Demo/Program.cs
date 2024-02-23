using Grecs.Demo.Components;
using System.Diagnostics;

namespace Grecs.Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var context = new EntityContext();
            var coordinator = new SystemCoordinator();


            var deltaTime = 0.001f;
            var nl = Environment.NewLine;
            Console.WriteLine("q to quit, anything else to iterate");

            long tick = 0;
            string input;
            while (true)
            {
                Console.Write("input: ");
                input = Console.ReadLine()?.Trim()?.ToLower() ?? "";
                if (input.StartsWith("q"))
                    break;
                else if (input.StartsWith("t"))
                {
                    RunPooledVsNonPooledTest();
                    continue;
                }
                else if (input.StartsWith("f"))
                {
                    PooledIntention.FlushInstances();
                    continue;
                } else if(input.StartsWith("g"))
                {
                    PooledIntention.FlushInstances();
                    RunPooledVsNonPooledGarbageCollectionTest();
                    continue;
                }

                // Process Tick
                tick++;
                Console.WriteLine($"-- Start Tick {tick} --{nl}");
                coordinator.Execute(deltaTime, context);
                Console.WriteLine($"{nl}-- End Tick {tick} --");
            }

            Console.WriteLine($"{nl}Ended after {tick} ticks. Bye!");
        }

        static void RunPooledVsNonPooledTest()
        {
            const int TEST_AMOUNT = 100000;
            var contextUsingPooled = new EntityContext();
            var isDone = false;

            var timer = new Stopwatch();
            var subTimer = new Stopwatch();
            timer.Restart();
            subTimer.Restart();
            // Seed with TEST_AMOUNT entities with the components
            for (int i = 0; i < TEST_AMOUNT; i++)
            {
                var pooledComponent = PooledIntention.GetInstance();
                pooledComponent.IsDone = isDone;
                contextUsingPooled.CreateEntity().AddComponent(pooledComponent);
                isDone = !isDone;
            }
            subTimer.Stop();
            Console.WriteLine($">>>> Seed: {subTimer.ElapsedMilliseconds}ms");
            subTimer.Restart();

            // Remove Some Change Some
            foreach(var entity in contextUsingPooled.GetEntities())
            {
                var component = entity.GetComponent<PooledIntention>();
                if (component.IsDone)
                    entity.RemoveComponent(component);
                else
                    component.IsDone = true;
            }
            subTimer.Stop();
            Console.WriteLine($">>>> Remove Some: {subTimer.ElapsedMilliseconds}ms");
            subTimer.Restart();

            // Remove Remaining
            foreach (var entity in contextUsingPooled.GetEntities())
            {
                if (entity.HasComponent(typeof(PooledIntention)))
                {
                    var component = entity.GetComponent<PooledIntention>();
                    entity.RemoveComponent(component);
                }
            }
            subTimer.Stop();
            Console.WriteLine($">>>> Remove All: {subTimer.ElapsedMilliseconds}ms");
            subTimer.Restart();

            // Readd to All
            foreach (var entity in contextUsingPooled.GetEntities())
            {
                var pooledComponent = PooledIntention.GetInstance();
                pooledComponent.IsDone = isDone;
                entity.AddComponent(pooledComponent);
                isDone = !isDone;
            }
            subTimer.Stop();
            Console.WriteLine($">>>> Readd All: {subTimer.ElapsedMilliseconds}ms");
            timer.Stop();
            subTimer.Restart();

            // Remove Remaining v2
            foreach (var entity in contextUsingPooled.GetEntities())
            {
                if (entity.HasComponent(typeof(PooledIntention)))
                {
                    var component = entity.GetComponent<PooledIntention>();
                    entity.RemoveComponent(component);
                }
            }
            subTimer.Stop();
            Console.WriteLine($">>>> Remove All2: {subTimer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Time for Pooled: {timer.ElapsedMilliseconds}ms");

            var contextUsingNonPooled = new EntityContext();
            isDone = false;
            timer.Restart();
            subTimer.Restart();
            // Seed with TEST_AMOUNT entities with the components
            for (int i = 0; i < TEST_AMOUNT; i++)
            {
                contextUsingNonPooled.CreateEntity().AddComponent(new NonPooledIntention
                {
                    IsDone = isDone
                });
                isDone = !isDone;
            }
            subTimer.Stop();
            Console.WriteLine($">>>> Seed: {subTimer.ElapsedMilliseconds}ms");
            subTimer.Restart();

            // Remove Some Change Some
            foreach (var entity in contextUsingNonPooled.GetEntities())
            {
                var component = entity.GetComponent<NonPooledIntention>();
                if (component.IsDone)
                    entity.RemoveComponent(component);
                else
                    component.IsDone = true;
            }
            subTimer.Stop();
            Console.WriteLine($">>>> Remove Some: {subTimer.ElapsedMilliseconds}ms");
            subTimer.Restart();

            // Remove Remaining
            foreach (var entity in contextUsingNonPooled.GetEntities())
            {
                if (entity.HasComponent(typeof(NonPooledIntention)))
                {
                    var component = entity.GetComponent<NonPooledIntention>();
                    entity.RemoveComponent(component);
                }
            }
            subTimer.Stop();
            Console.WriteLine($">>>> Remove All: {subTimer.ElapsedMilliseconds}ms");
            subTimer.Restart();

            // Readd to All
            foreach (var entity in contextUsingNonPooled.GetEntities())
            {
                entity.AddComponent(new NonPooledIntention
                {
                    IsDone = isDone
                });
                isDone = !isDone;
            }
            subTimer.Stop();
            Console.WriteLine($">>>> Readd All: {subTimer.ElapsedMilliseconds}ms");
            timer.Stop();
            subTimer.Restart();

            // Remove Some Change Some
            foreach (var entity in contextUsingNonPooled.GetEntities())
            {
                if (entity.HasComponent(typeof(NonPooledIntention)))
                {
                    var component = entity.GetComponent<NonPooledIntention>();
                    entity.RemoveComponent(component);
                }
            }
            subTimer.Stop();
            Console.WriteLine($">>>> Remove all 2: {subTimer.ElapsedMilliseconds}ms");
            Console.WriteLine($"Time for Non-Pooled: {timer.ElapsedMilliseconds}ms");

        }

        static void RunPooledVsNonPooledGarbageCollectionTest()
        {
            var timer = new Stopwatch();

            timer.Restart();
            Console.WriteLine("-- start POOLED --");
            var contextUsingPooled = new EntityContext();
            var entityUsingPooled = contextUsingPooled.CreateEntity();
            while (timer.ElapsedMilliseconds < 30000)
            {
                if (entityUsingPooled.HasComponent(typeof(PooledIntention)))
                {
                    var component = entityUsingPooled.GetComponent<PooledIntention>();
                    entityUsingPooled.RemoveComponent(component);
                }

                entityUsingPooled.AddComponent(PooledIntention.GetInstance());
            }
            Console.WriteLine("-- end POOLED --");
            timer.Stop();


            timer.Restart();
            Console.WriteLine("-- start NON-POOLED --");
            var contextUsingNonPooled = new EntityContext();
            var entityUsingNonPooled = contextUsingNonPooled.CreateEntity();
            while (timer.ElapsedMilliseconds < 30000)
            {
                if (entityUsingNonPooled.HasComponent(typeof(NonPooledIntention)))
                {
                    var component = entityUsingNonPooled.GetComponent<NonPooledIntention>();
                    entityUsingNonPooled.RemoveComponent(component);
                }

                entityUsingNonPooled.AddComponent(new NonPooledIntention());
            }
            Console.WriteLine("-- end NON-POOLED --");
            timer.Stop();
        }
    }
}
