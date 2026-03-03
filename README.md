# Sampling Tool
## Terrain Generation using Multi-Layered Perlin Noise
I followed Sebastian Lague's **Procedural Terrain Generation** series on youtube:\
[Link to the youtube playlist](https://www.youtube.com/watch?v=wbpMiKiSKm8&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3).

It is from this series I learned the basic concepts of Perlin Noise and how it could be used in Unity.\
I used multilayered perlin noise, which is basically a couple of perlin noise outputs layered on top of each other to create more detail the more layers are added.\
The terrain generator has values for Octaves, Persistence and Lacunarity, all of which affect how the noise is implemented.\

## Async Poisson Disk Sampling
I started with a simple implementation of Poisson sampling, knowing that it was probably going to be slow. I then came across blogs online where it was implemented asynchronously across multiple spatially partitioned instances, and that's what I went for:
- Initially, there would be a trigger collider that would act as the sampling volume.
- I would split the volume in X-Z plane into spatial partitions recursively, and start a poisson sampling instance with the boundaries of each partition. I would also run each partition asynchronously. 

