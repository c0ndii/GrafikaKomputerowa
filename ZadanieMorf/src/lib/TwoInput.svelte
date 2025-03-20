<script lang="ts">
	type Params = {
		param1: number;
		param2: number;
	};

	type Props = {
		onChange: (value: Params) => void;
		min?: number[];
		max?: number[];
		step?: number[];
		label1: string;
		label2: string;
	};

	const {
		onChange,
		min = [0, 0],
		max = [255, 255],
		step = [1, 1],
		label1,
		label2
	}: Props = $props();

	let color = $state<Params>({ param1: 0, param2: 0 });
</script>

<div class="flex flex-row items-center">
	<div class="flex flex-col items-center m-1">
		<div class="flex flex-col items-center border border-black">
			<label for="param1">{label1}</label>
			<div class="flex flex-row m-1">
				{min[0]}
				<input
					id="param1"
					step={step[0]}
					type="range"
					min={min[0]}
					max={max[0]}
					bind:value={color.param1}
				/>
				{max[0]}
			</div>
			{color.param1}
		</div>
		<div class="flex flex-col items-center border border-black">
			<label for="param2">{label2}</label>
			<div class="flex flex-row m-1">
				{min[1]}
				<input
					id="param2"
					step={step[1]}
					type="range"
					min={min[1]}
					max={max[1]}
					bind:value={color.param2}
				/>
				{max[1]}
			</div>
			{color.param2}
		</div>
	</div>
	<button onclick={() => onChange(color)}>Render</button>
</div>
