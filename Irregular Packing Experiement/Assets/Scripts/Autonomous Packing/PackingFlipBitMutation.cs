using System;
using System.ComponentModel;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using UnityEngine;

namespace GeneticSharp.Domain.Mutations
{
    /// <summary>
    /// Takes the chosen genome and inverts the bits (i.e. if the genome bit is 1, it is changed to 0 and vice versa).
    /// </summary>
    /// <remarks>
    /// When using this mutation the genetic algorithm should use IBinaryChromosome.
    /// </remarks>
    [DisplayName("Flip Bit")]
    public class PackingFlipBitMutation : MutationBase
    {
        #region Fields
        private readonly IRandomization m_rnd;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticSharp.Domain.Mutations.FlipBitMutation"/> class.
        /// </summary>
        public PackingFlipBitMutation ()
        {
            m_rnd = RandomizationProvider.Current;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Mutate the specified chromosome.
        /// </summary>
        /// <param name="chromosome">The chromosome.</param>
        /// <param name="probability">The probability to mutate each chromosome.</param>
        protected override void PerformMutate (IChromosome chromosome, float probability)
        {
            var packingChromosome = chromosome as PackingChromosome;

            if (packingChromosome == null) 
            {
                throw new MutationException (this, "Needs a packing chromosome that implements PackingChromosome.");    
            }

            if (m_rnd.GetDouble() <= probability)
            {
                var index = m_rnd.GetInt(0, packingChromosome.Length);
                // Number of bits to flip
                var num_mutate = m_rnd.GetInt(1, 4);
                // indexes to flip (based on the number of bits to flip)
                int[] mutate_indexes = m_rnd.GetUniqueInts(num_mutate, 0, 3);
                packingChromosome.FlipSpecificGeneValues(index, mutate_indexes);
            }
        }
        #endregion
    }
}