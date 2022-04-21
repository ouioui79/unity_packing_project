using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Reinsertions;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GAController : MonoBehaviour
{
	private Thread m_gaThread;
	private GeneticAlgorithm m_ga;
	
	private int num_obj;

	private int m_currentGenerationNumber;

    //Parameters of the Genetic Algorithm (can be changed)
	public int m_populationSize = 50;
	public int m_generationNumber = 30;
	public float p_mutation = 0.2f;
	public float p_crossover = 0.75f;

	private Population Population;
	private EliteSelection Selection;
	private PackingOrderBasedCrossover Crossover;
	private PackingFlipBitMutation Mutation;
	private ElitistReinsertion Reinsertion;

	public bool has_started = false;
	public bool finished = false;
	
	Stopwatch watch = new Stopwatch();
	Stopwatch GAwatch = new Stopwatch();

	private PackingChromosome best;

    private bool initialSetupMethods = false;

	#region Methods

	public void SetUp(int num_objects)
	{
		num_obj = num_objects;//GameObject.FindGameObjectsWithTag("Interactable").Length;
        //PackingChromosome adam = new PackingChromosome(num_obj, 0, num_obj,gameObject.GetComponent<BLFPacking>()); 
        PackingChromosome adam = new PackingChromosome(num_obj, 0, num_obj, gameObject.GetComponent<BLFPackingInwards>());
        Population = new Population(m_populationSize, m_populationSize, adam);
		Selection = new EliteSelection();
		Crossover = new PackingOrderBasedCrossover();
		Mutation = new PackingFlipBitMutation();
		Reinsertion = new ElitistReinsertion();
	}

	private void Update()
	{
		if (has_started && !finished) //Where all the Genetic Algorithm process happens
		{
           
			if (Population.GenerationsNumber == 0)
			{
				GAwatch.Start();
				Population.CreateInitialGeneration();
			}
			
			if (Population.GenerationsNumber <= m_generationNumber)
			{
				Debug.LogFormat("GENERATION #{0}", Population.GenerationsNumber);
				watch.Start();
				foreach (PackingChromosome c in Population.CurrentGeneration.Chromosomes)
				{
					c.Evaluate(false); //Is called for each chromosome
					/*Debug.Log("--------CHROMOSOME--------");
					c.PrintChromosome();
					Debug.Log("CURRENT BEST FITNESS: " + c.Fitness);
					Debug.Log("-------------------------------");*/
				}

				Population = EvolveOneGeneration(Population);
				best = (PackingChromosome)Population.BestChromosome;
				watch.Stop();
				
				/*Debug.Log("--------CURRENT BEST CHROMOSOME--------");
				best.PrintChromosome();
				Debug.Log("CURRENT BEST FITNESS: " + best.Fitness);
				Debug.Log("-------------------------------");
                Debug.Log("GENERATION EXECUTION TIME " + watch.Elapsed.ToString(@"mm\:ss\.ff"));*/
				watch.Reset();
			} else if (Population.GenerationsNumber > m_generationNumber && !finished)
			{
				finished = true;
				GAwatch.Stop();

				//Debug.Log("--------BEST CHROMOSOME--------");
				//best.PrintChromosome();
				//Debug.Log("BEST FITNESS: " + best.Fitness);
				//Debug.Log("-------------------------------");
				Vector3 iResults = best.Evaluate(true); //Will run the BLF Packing with the chromosome that has the best fitness

                Debug.Log("--------FINAL RESULTS--------");
                Debug.Log("# of generations: " + m_generationNumber + ", population size: " + m_populationSize);

                string blfType;
                if (best.packer.outwards) blfType = "Outwards BLF";
                else blfType = "Inwards BLF";

                Debug.Log(blfType + "Results: 1.Pack. Eff.: " + iResults.z + ", 2. Num. of Obj packed: " + best.packer.colliders_counter.count);
                Debug.Log("GA EXECUTION TIME " + GAwatch.Elapsed.ToString(@"hh\:mm\:ss\.ff"));
			}
		}
	}

	public void StartGA() //Called to start the process of the genetic algorithm, started when the button is pressed in the UI
	{
		if (!has_started)
		{
			has_started = true;
		}
	}

	public bool Finished()
	{
		return finished;
	}

	private Population EvolveOneGeneration(Population p)
    {
	    var parents = Selection.SelectChromosomes(p.MinSize, p.CurrentGeneration);
	    
	    var minSize = Population.MinSize;
	    var offsprings = new List<IChromosome>(minSize);
	    
	    for (int i = 0; i < minSize; i += Crossover.ParentsNumber)
	    {
		    var selectedParents = parents.Skip(i).Take(Crossover.ParentsNumber).ToList();
		    
		    if (selectedParents.Count == Crossover.ParentsNumber && RandomizationProvider.Current.GetDouble() <= p_crossover)
		    {
			    var children = Crossover.Cross(selectedParents);
			    /*Debug.Log("========Parents=========");
			    ((PackingChromosome)selectedParents[0]).PrintChromosome();
			    ((PackingChromosome)selectedParents[1]).PrintChromosome();
			    Debug.Log("========Children=========");
			    ((PackingChromosome)children[0]).PrintChromosome();
			    ((PackingChromosome)children[1]).PrintChromosome();*/
			    if (children != null)
			    {
				    offsprings.AddRange(children);
			    }
		    }
	    }
	    
	    for (int i = 0; i < offsprings.Count; i++)
	    {
		    Mutation.Mutate(offsprings[i], p_mutation);
	    }
	    
	    Reinsertion.SelectChromosomes(p, offsprings, parents);
		
	    p.EndCurrentGeneration();
	    p.CreateNewGeneration(offsprings);
	    
	    return p; 
    }

    #endregion
}