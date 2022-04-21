using UnityEngine;
using System;
using System.Linq;
using GeneticSharp.Infrastructure.Framework.Texts;
using GeneticSharp.Infrastructure.Framework.Commons;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;

public struct ObjectStruct
{
    public int id;
    public GameObject obj;
    public Vector3 orig_pos;
    public Vector3 orig_rot;

    public ObjectStruct(int id_, GameObject obj_, Vector3 pos_, Vector3 rot_)
    {
        id = id_;
        obj = obj_;
        orig_pos = pos_;
        orig_rot = rot_;
    }
}
public sealed class PackingChromosome : IChromosome
{
    #region Fields
    private Gene[] m_genes;
    private int m_length;
    
    private readonly int m_minValue;
    private readonly int m_maxValue;
    public BLFPackingInwards packer;
    #endregion

    #region Constructors        
    /// <summary>
    /// Initializes a new instance of the <see cref="ChromosomeBase"/> class.
    /// </summary>
    /// <param name="length">The length, in genes, of the chromosome.</param>
    public PackingChromosome(int length, int minValue, int maxValue, BLFPackingInwards packer_)
    {
        ValidateLength(length);

        m_length = length;
        m_genes = new Gene[length];
        m_minValue = minValue;
        m_maxValue = maxValue;
        packer = packer_;
        CreateGenes();
    }
    #endregion

    #region Properties

    public double? Fitness { get; set; }

    public int Length
    {
        get
        {
            return m_length;
        }
    }
    #endregion

    #region Methods
    public Gene GenerateGene(int geneIndex)
    {
        int[] geneValue = new int[4];
        
        // object ID
        geneValue[0] = geneIndex;//RandomizationProvider.Current.GetInt(m_minValue, m_maxValue+1);
        
        // ry bits
        geneValue[1] = RandomizationProvider.Current.GetInt(0, 2);
        geneValue[2] = RandomizationProvider.Current.GetInt(0, 2);
        geneValue[3] = RandomizationProvider.Current.GetInt(0, 2);
        /*
        // rx bits
        geneValue[4] = RandomizationProvider.Current.GetInt(0, 2);
        geneValue[5] = RandomizationProvider.Current.GetInt(0, 2);
        geneValue[6] = RandomizationProvider.Current.GetInt(0, 2);

        // rz bits
        geneValue[7] = RandomizationProvider.Current.GetInt(0, 2);
        geneValue[8] = RandomizationProvider.Current.GetInt(0, 2);
        geneValue[9] = RandomizationProvider.Current.GetInt(0, 2);
        */
        return new Gene(geneValue);
    }

    public Gene GenerateSpecificGene(int geneIndex, int b1, int b2, int b3)
    {
        int[] geneValue = {geneIndex, b1, b2, b3};
        return new Gene(geneValue);
    }

    IChromosome IChromosome.CreateNew()
    {
        return CreateNew();
    }
    public PackingChromosome CreateNew()
    {
        return new PackingChromosome(m_length, m_minValue, m_maxValue, packer);
    }

    public IChromosome Clone()
    {
        var clone = CreateNew();
        clone.ReplaceGenes(0, GetGenes());
        clone.Fitness = Fitness;

        return clone;
    }
    public void ReplaceGene(int index, Gene gene)
    {
        if (index < 0 || index >= m_length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "There is no Gene on index {0} to be replaced.".With(index));
        }

        m_genes[index] = gene;
        Fitness = null;
    }

    public void ReplaceGenes(int startIndex, Gene[] genes)
    {
        ExceptionHelper.ThrowIfNull("genes", genes);

        if (genes.Length > 0)
        {
            if (startIndex < 0 || startIndex >= m_length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "There is no Gene on index {0} to be replaced.".With(startIndex));
            }

            var genesToBeReplacedLength = genes.Length;

            var availableSpaceLength = m_length - startIndex;

            if (genesToBeReplacedLength > availableSpaceLength)
            {
                throw new ArgumentException(
                    nameof(Gene),
                    "The number of genes to be replaced is greater than available space, there is {0} genes between the index {1} and the end of chromosome, but there is {2} genes to be replaced."
                    .With(availableSpaceLength, startIndex, genesToBeReplacedLength));
            }

            Array.Copy(genes, 0, m_genes, startIndex, genes.Length);

            Fitness = null;
        }
    }
    
    public void Resize(int newLength)
    {
        ValidateLength(newLength);

        Array.Resize(ref m_genes, newLength);
        m_length = newLength;
    }
    
    public Gene GetGene(int index)
    {
        return m_genes[index];
    }
    public Gene[] GetGenes()
    {
        return m_genes;
    }
    public int CompareTo(IChromosome other)
    {
        if (other == null)
        {
            return -1;
        }

        var otherFitness = other.Fitness;

        if (Fitness == otherFitness)
        {
            return 0;
        }

        return Fitness > otherFitness ? 1 : -1;
    }

    public override bool Equals(object obj)
    {
        var other = obj as IChromosome;

        if (other == null)
        {
            return false;
        }

        return CompareTo(other) == 0;
    }

    public override int GetHashCode()
    {
        return Fitness.GetHashCode();
    }
    
    private void CreateGene(int index)
    {
        ReplaceGene(index, GenerateGene(index));
    }
     
    private void CreateGenes()
    {
        var obj_indexes = RandomizationProvider.Current.GetUniqueInts(Length, m_minValue, m_maxValue);
        for (int i = 0; i < Length; i++)
        {
            ReplaceGene(i, GenerateGene(obj_indexes[i]));
        }
    }

    private static void ValidateLength(int length)
    {
        if (length < 2)
        {
            throw new ArgumentException("The minimum length for a chromosome is 2 genes.", nameof(length));
        }
    }
    
    public Vector3 Evaluate(bool print)
    {
        packer.SetUpPacking(this);
        Vector3 properties = packer.RunBLFAbstract();

        if (properties.x > 1f && properties.y > 1f)
        {
            if (print)
            {
                Debug.Log("WEIGHT AND RADIATION EXCEEDED");
            }
            properties.z -= 0.15f;
        } else if (properties.x > 1f || properties.y > 1f)
        {
            if (print && properties.x > 1f)
            {
                Debug.Log("WEIGHT EXCEEDED");
            } else if (print && properties.y > 1f)
            {
                Debug.Log("RADIATION EXCEEDED");
            }
            properties.z -= 0.1f;
        }

        if (print) Debug.Log(properties);
        this.Fitness = properties.z;
        return properties;
    }

    public void FlipSpecificGeneValues(int index, int[] flip_indexes)
    {
        var value = GetGene (index).Value as int[];

        int[] new_bits = new int[3];
        for (int i = 0; i < 3; i++)
        {
            if (flip_indexes.Contains(i))
            {
                new_bits[i] = value[1+i] == 0 ? 1 : 0;
            }
            else
            {
                new_bits[i] = value[1+i];
            }
        }

        Gene new_gene = GenerateSpecificGene(value[0], new_bits[0], new_bits[1], new_bits[2]);
        var print_gene = new_gene.Value as int[];
        
        ReplaceGene (index, new_gene);
    }
    
    public void FlipGene (int index)    
    {
        var value = GetGene (index).Value as int[];
        int b1 = value[1] == 0 ? 1 : 0;
        int b2 = value[2] == 0 ? 1 : 0;
        int b3 = value[3] == 0 ? 1 : 0;

        Gene new_gene = GenerateSpecificGene(value[0], b1, b2, b3);

        ReplaceGene (index, new_gene);
    }
    
    public void PrintChromosome()
    {
        string print_msg = "\n";
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < this.Length; j++)
            {
                int[] geneValue = this.GetGene(j).Value as int[];
                print_msg += geneValue[i];
                if (j < this.Length - 1)
                {
                    print_msg += ",";
                }
            }
            print_msg+="\n";
        }

        Debug.Log(print_msg);
    }
    
    #endregion
}
