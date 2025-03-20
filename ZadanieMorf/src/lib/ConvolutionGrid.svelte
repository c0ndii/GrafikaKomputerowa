<script lang="ts">
	type Props = {
		onSubmit: (matrix: number[][], operation: 'min' | 'max') => void;
	};

	const { onSubmit }: Props = $props();

	let width = $state(3);
	let height = $state(3);

	let matrix = $state<number[][]>([]);
	let useSliders = $state(false);

	let isMin = $state(false);

	let operation = $derived<'min' | 'max'>(isMin ? 'min' : 'max');

	$effect(() => {
		matrix = Array.from({ length: height }, () => Array.from({ length: width }, () => 0));
	});
</script>

<div>
	<div class="nice flex flex-col m-1 items-center w-8">
		<label for="width">Width:</label>
		<input id="width" type="number" min="1" bind:value={width} />
		<label for="height">Height:</label>
		<input id="height" type="number" min="1" bind:value={height} />
		<div class="flex flex-row">
			Using {isMin ? 'Min' : 'Max'} operation
			<input type="checkbox" bind:checked={isMin} />
		</div>
		<div class="flex flex-row p-1">
			Use sliders:
			<input type="checkbox" bind:checked={useSliders} />
		</div>
		<button onclick={() => onSubmit(matrix, operation)}>Render</button>
	</div>

	<table class="nice">
		{#each matrix as inner, i}
			<tbody>
				<tr>
					{#each inner as _, j}
						<td>
							{#if useSliders}
								<div class="border border-black p-1 flex flex-col items-center content-center">
									<input type="range" min="-1" max="1" step="0.1" bind:value={matrix[i][j]} />
									{matrix[i][j]}
								</div>
							{:else}
								<input class="w-10" type="number" step="0.1" bind:value={matrix[i][j]} />
							{/if}
						</td>
					{/each}
				</tr>
			</tbody>
		{/each}
	</table>
</div>

<style>
	.nice {
		border-collapse: collapse;
		margin: 5px 0;
		font-size: 0.9em;
		font-family: sans-serif;
		min-width: 400px;
		box-shadow: 0 0 20px rgba(0, 0, 0, 0.15);
	}

	td {
		padding: 5px 5px;
	}
</style>
