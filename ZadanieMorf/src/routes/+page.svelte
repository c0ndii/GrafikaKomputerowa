<script lang="ts">
	import ConvolutionGrid from '$lib/ConvolutionGrid.svelte';
	import {
		ClosingFilter,
		CustomElement,
		DilationFilter,
		ErosionFilter,
		HitOrMissFilter,
		NoopFilter,
		OpeningFilter,
		type ImageFilter
	} from '$lib/FilterProvider';
	import { ImageRenderer } from '$lib/ImageRenderer';

	let canvas!: HTMLCanvasElement;
	let ctx!: CanvasRenderingContext2D;
	let image!: HTMLImageElement;

	let files: FileList | null = $state(null);
	let imgtag!: HTMLImageElement;

	enum FilterType {
		Noop,
		Dilation,
		Erosion,
		Opening,
		Closing,
		HitOrMiss,
		Custom
	}

	const filterParamCount = {
		[FilterType.Noop]: 0,
		[FilterType.Dilation]: 0,
		[FilterType.Erosion]: 0,
		[FilterType.Opening]: 0,
		[FilterType.Closing]: 0,
		[FilterType.HitOrMiss]: 0
	};

	const noParamFactory: any = {
		[FilterType.Noop]: () => new NoopFilter(),
		[FilterType.Dilation]: () => new DilationFilter(),
		[FilterType.Erosion]: () => new ErosionFilter(),
		[FilterType.Opening]: () => new OpeningFilter(),
		[FilterType.Closing]: () => new ClosingFilter(),
		[FilterType.HitOrMiss]: () => new HitOrMissFilter()
	} as any;

	let filter = $state<ImageFilter>(new NoopFilter());
	let filterType = $state(FilterType.Noop);
	let baseImage: ImageRenderer | null;

	$inspect(filter);

	function performRender() {
		baseImage?.render(filter);
	}

	$effect(() => {
		const ctx2d = canvas.getContext('2d');
		if (ctx2d === null) {
			alert('Canvas not supported');
			return;
		}
		ctx = ctx2d;
		baseImage = new ImageRenderer(imgtag, canvas, ctx);
		imgtag.onload = () => {
			baseImage?.setNewImage(imgtag);
			baseImage?.render(filter);
		};
	});

	$effect(() => {
		if (files) {
			const file: File = files[0];
			imgtag.src = URL.createObjectURL(file);
			ctx.drawImage(imgtag, 0, 0);
		}
	});

	let selectedFilter = $state(0);

	$effect(() => {
		switch (+selectedFilter) {
			case 0:
				filterType = FilterType.Noop;
				break;
			case 1:
				filterType = FilterType.Dilation;
				break;
			case 2:
				filterType = FilterType.Erosion;
				break;
			case 3:
				filterType = FilterType.Opening;
				break;
			case 4:
				filterType = FilterType.Closing;
				break;
			case 5:
				filterType = FilterType.HitOrMiss;
				break;
		}
	});
</script>

<main>
	<input type="file" accept="image/*" bind:files />
	<button
		onclick={() => {
			const a = document.createElement('a');
			a.href = canvas.toDataURL();
			a.download = 'image.png';
			a.click();
		}}>Download</button
	>

	<div class="flex flex-row">
		<img class="border border-black" bind:this={imgtag} alt="zdjecie" />
		<canvas class="border border-black" bind:this={canvas}></canvas>
	</div>
	<select bind:value={selectedFilter}>
		<option value="0">{FilterType[0]}</option>
		<option value="1">{FilterType[1]}</option>
		<option value="2">{FilterType[2]}</option>
		<option value="3">{FilterType[3]}</option>
		<option value="4">{FilterType[4]}</option>
		<option value="5">{FilterType[5]}</option>
	</select>

	{#if filterType === FilterType.Custom}
		{'huh'}
		<ConvolutionGrid
			onSubmit={(matrix, op) => {
				filter = new CustomElement(matrix, op);
				performRender();
			}}
		/>
	{:else if filterParamCount[filterType] === 0}
		<button
			onclick={() => {
				filter = noParamFactory[filterType]();
				performRender();
			}}
		>
			Render
		</button>
	{/if}
</main>

<style>
	img {
		display: block;
		max-width: 300px;
		max-height: 300px;
		width: 300px;
		height: 300px;
	}

	canvas {
		display: block;
		max-width: 300px;
		max-height: 300px;
		width: 300px;
		height: 300px;
	}
</style>
