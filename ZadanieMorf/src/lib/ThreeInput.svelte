<script lang="ts">
	type Params = {
		param1: number;
		param2: number;
		param3: number;
	};

	type Props = {
		onChange: (value: Params) => void;
		min?: number[];
		max?: number[];
		step?: number[];
		label1: string;
		label2: string;
		label3: string;
	};

	const {
		onChange,
		min = [0, 0, 0],
		max = [255, 255, 255],
		step = [1, 1, 1],
		label1,
		label2,
		label3
	}: Props = $props();

	let param = $state<Params>({ param1: 0, param2: 0, param3: 0 });
</script>

<div class="flex flex-row items-center">
	<div class="flex flex-col items-center m-1">
		<div class="flex flex-col items-center border border-black">
			<label for="label1">{label1}</label>
			<div class="flex flex-row m-1">
				{min[0]}
				<input
					id="label1"
					step={step[0]}
					type="range"
					min={min[0]}
					max={max[0]}
					bind:value={param.param1}
				/>
				{max[0]}
			</div>
			{param.param1}
		</div>
		<div class="flex flex-col items-center border border-black">
			<label for="label2">{label2}</label>
			<div class="flex flex-row m-1">
				{min[1]}
				<input
					id="label2"
					step={step[1]}
					type="range"
					min={min[1]}
					max={max[1]}
					bind:value={param.param2}
				/>
				{max[1]}
			</div>
			{param.param2}
		</div>
		<div class="flex flex-col items-center border border-black">
			<label for="label3">{label3}</label>
			<div class="flex flex-row m-1">
				{min[2]}
				<input
					id="label3"
					step={step[2]}
					type="range"
					min={min[2]}
					max={max[2]}
					bind:value={param.param3}
				/>
				{max[2]}
			</div>
			{param.param3}
		</div>
	</div>
	<button onclick={() => onChange(param)}>Render</button>
</div>
