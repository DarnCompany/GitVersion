using Build;
using Cake.Frosting;
using Common.Lifetime;

new CakeHost()
    .UseContext<BuildContext>()
    .UseLifetime<BuildLifetime>()
    .UseTaskLifetime<BuildTaskLifetime>()
    .UseWorkingDirectory("../..")
    .Run(args);
