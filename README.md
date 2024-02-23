## Grecs

### What is this?
> It's a thing that does stuff.

### How do I use it?

1. Add the [nuget package](https://www.nuget.org/packages/Grecs) as a dependency
2. See code example below
3. Enjoy!


## Code Examples

### Setup a Context

The `EntityContext` is the main state container for Grecs.
Generally you only need one.

```csharp
var context = new Grecs.EntityContext();
```

### Create a System or two

There are two popular systems in Grecs, a `Grecs.TickSystem` and a `Grecs.TriggerSystem`

TickSystems are processed every tick, TriggerSystem are only processed in response to Add/Edit/Removal of components from an entity during a tick.


### Create Some Components

Components are just sacks of data attached to Entitys, there is some philosophy around what should and shouldn't be a component; but you do you.

For Components to be built effectively and plug in smoothly to the various EntityQuery and listeners you gotta do some weird Property hackery, here's an example:

```csharp
    internal class Health: Grecs.Component
    {
        private int _maxHealth = 4;
        private int _health = 4;

        // I promise I'll fix this weird syntax (no promises)
        public int MaxHP
        {
            get => _maxHealth; set
            {
                if (_maxHealth != value)
                {
                    _maxHealth = value;
                    // this is key  to allow trigger systems to respond to changes
                    Owner?.TriggerComponentChanged(this);
                }
            }
        }

        public int HP
        {
            get => _health; set
            {
                if (_health != value)
                {
                    _health = value;
                    Owner?.TriggerComponentChanged(this);
                }
            }
        }
    }
```

### Create some Entities

```csharp

var context = new Grecs.EntityContext();
var someEntity = _context.CreateEntity();
someEntity.AddComponent(new Health {
    Health = 18,
    MaxHealth = 32,
});

```


### Setup a SystemCoordinator

The `SystemCoordinator` is the coordinator of the systems that run against the `EntityContext`. In essence this setups up the `Entity` and `Component` processing pipeline.

```csharp
var context = new Grecs.EntityContext();
var gameCoordinator = new Grecs.SystemCoordinator();

// Attach YOUR systems to the coordinator
gameCoordinator
    .Add(new AnimatorSystem())
    .Add(new InputTickSystem())
    .Add(new SlimeSystem())
    .Add(new AttackTickSystem())
    .Add(new MovementSystem(context))
    .Add(new DeathSystem(context))
    .Add(new SkeletonSystem(context))
    .Add(new GraphicsSystem(context));
```

### Run the Systems

Now that Grecs is configured, simple tick the coordinator to process the systems.

```csharp
var context = new Grecs.EntityContext();
var gameCoordinator = new Grecs.SystemCoordinator();

// ...

// in your update function
gameCoordinator.Execute(deltaTime, context);
```

### Quick word on EntityQuery's

Queries support basic And and Or based on the Component Types, this is most easily explained by example

```csharp
var query = new Grecs.EntityQuery();
query.And(typeof(Health), typeof(Player));

var context = new Grecs.EntityContext();
// use the query to pull entities
var healthAndPlayerEntities = context.GetEntities(query);
```

### An additional word on Listeners

These are mostly used for trigger systems.
```csharp
var query = new Grecs.EntityQuery();
query.And(typeof(Health), typeof(Player));

var context = new Grecs.EntityContext();

// create a listener that will react to added or changed Health and Player components (these work due to the funky Component property syntax shown way above)
var healthAndPlayerListener = context.CreateListener(query);
healthAndPlayerListener.ListenToAdded = true;
healthAndPlayerListener.ListenToChanged = true;
healthAndPlayerListener.ListenToRemoved = false;

// ... run Systems, or add, remove, update compoents on entities ...

var addedOrChangedEntities = healthAndPlayerListener.Gather();

```


### Example Tick System

```csharp
internal class HiTickSystem : Grecs.TickSystem
{
    // Implemented interface function
    public override void Execute(float deltaTime, EntityContext context)
    {
        Console.WriteLine("Hi");
    }
}
```

### Example Trigger System

```csharp
internal class HiTriggerSystem : Gercs.TriggerSystem
{
    // Constructor required
    public HiTriggerSystem(EntityContext context):base(context) { }

    // Implemented from interface
    public override void Execute(EntityContext context, Entity[] entities)
    {
        foreach(var entity in entities)
        {
            Console.WriteLine($"Hi {entity.id}");
        }
    }

    // Implement from interface
    protected override bool Filter(Entity entity)
    {
        // can do additional filtering against specific entities that are pulled from the listeners
        return true;
    }

    protected override QueryListener GetListener(EntityContext context)
    {
        var hiQuery = new EntityQuery();
        hiQuery.And(typeof(HiComponent));
        var listener = context.CreateListener(hiQuery);
        listener.ListenToAdded = true;
        listener.ListenToChanged = true;
        listener.ListenToRemoved = true;

        return listener;
    }
}
```


### A Full Executable Example

```csharp
using Grecs;

namespace GrecsExample
{
    internal class Program
    {
        /**
         * A Program that:
         *  - Creates 5 entities
         *  - Updates their Component every frame till it is at or above 5
         *  - Prints the number of components updated every frame
         *  
         * Observations:
         *  - TickSystems run every frame
         *  - TriggerSystems only run when triggered
         */
        static void Main(string[] args)
        {
            // Setup Context and Systems
            var context = new Grecs.EntityContext();
            var gameCoordinator = new Grecs.SystemCoordinator();
            gameCoordinator
                .Add(new HiTickSystem())
                .Add(new HiTriggerSystem(context));

            // create 5 entities with different SomeNumbers
            for(var  i = 0; i < 5; i++)
            {
                var entity = context.CreateEntity();
                entity.AddComponent(new HiComponent { SomeNumber = i });
            }

            // generally you'd get the time between frames, but for simplicty this is hardcoded
            var deltaTime = .016f;

            Console.WriteLine("Let us get this party started.");
            for(var i = 0; i < 15; i++)
            {
                Console.WriteLine($"Tick {i}");
                gameCoordinator.Execute(deltaTime, context);
            }
        }
    }

    internal class HiComponent : Grecs.Component
    {
        private int _someNumber;
        public int SomeNumber
        {
            get => _someNumber; set
            {
                if (_someNumber != value)
                {
                    _someNumber = value;
                    Owner?.TriggerComponentChanged(this);
                }
            }
        }
    }

    internal class HiTickSystem : Grecs.TickSystem
    {
        public override void Execute(float deltaTime, EntityContext context)
        {
            var hiQuery = new EntityQuery();
            hiQuery.And(typeof(HiComponent));

            // get all entities with HiComponents and increase SomeNumber until it reaches 5
            var hiEntities = context.GetEntities(hiQuery);
            foreach (var hiEntity in hiEntities)
            {
                var hiComponent = hiEntity.GetComponent<HiComponent>();
                if (hiComponent.SomeNumber < 5)
                {
                    hiComponent.SomeNumber++;
                }
            }
        }
    }

    internal class HiTriggerSystem : Grecs.TriggerSystem
    {
        // pass associated context to bad Grecs.TriggerSystem
        public HiTriggerSystem(EntityContext context) : base(context){}

        public override void Execute(EntityContext context, Entity[] entities)
        {
            Console.WriteLine($"# of Entites Changed: {entities.Length}");
        }

        protected override bool Filter(Entity entity)
        {
            return true;
        }

        protected override QueryListener GetListener(EntityContext context)
        {
            var hiQuery = new EntityQuery();
            hiQuery.And(typeof(HiComponent));

            // Listent to changes to HiComponents
            var listener = context.CreateListener(hiQuery);
            listener.ListenToAdded = false;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = false;

            return listener;
        }
    }
}

```


### Regular Component vs PooledComponent

Use the `Grecs.PooledComponent` for any component that is constantly created and destroyed, otherwise extend the regular `Grecs.Component`

The only functional difference between the two is your options for instantiation.
For regular ol `Grecs.Component` you can `new` them up, or call `CreateComponent` on a `Grecs.Entity`
For the `Grecs.PooledComponent` you  MUST use `[Your New PooledComponent sub class].GetInstance()` for the entity `CreateComponent`.

Enjoy this small snippet

```csharp
internal class MyRegularOldComponent: Component
{
    private bool _isDone;
    public bool IsDone
    {
        get => _isDone; set
        {
            if (_isDone != value)
            {
                _isDone = value;
                Owner?.TriggerComponentChanged(this);
            }
        }
    }
}

internal class MyFancyPooledVersion: PooledComponent<PooledIntention>
{
    private bool _isDone;
    public bool IsDone
    {
        get => _isDone; set
        {
            if (_isDone != value)
            {
                _isDone = value;
                Owner?.TriggerComponentChanged(this);
            }
        }
    }
}

/// ... in your instantiation function

var context = new Grecs.EntityContext();

var entity = context.CreateEntity();

// create a regular old component
entity.AddComponent(new MyRegularOldComponent { IsDone = false });

// creating a fancy pooled one, no fancy Object Intializer syntax :(
var c = MyFancyPooledVersion.GetInstace();
c.IsDone = true;
entity.AddComponent(c);


```
