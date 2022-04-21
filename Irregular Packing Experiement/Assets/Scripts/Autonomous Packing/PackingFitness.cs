using System;
using System.Collections;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using UnityEngine;
using GeneticSharp.Domain.Fitnesses;

public class PackingFitness : IFitness
{
    private BLFPacking packer;

    public PackingFitness(BLFPacking packer_)
    {
        packer = packer_;
    }
    public double Evaluate(IChromosome chromosome)
    {
        var c = chromosome as PackingChromosome;
        packer.SetUpPacking(c);
        Vector3 properties = packer.RunBLF();

        if (properties.x > 1f && properties.y > 1f)
        {
            properties.z -= 0.15f;
        } else if (properties.x > 1f || properties.y > 1f)
        {
            properties.z -= 0.1f;
        }

        return properties.z;
    }
}
