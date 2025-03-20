<script lang="ts">
	type Props = {
		onChange: (value: number) => void;
		label: string;
		min: number;
		max: number;
		step: number;
	};

	const { onChange, label, min, max, step }: Props = $props();

	let value = $state<number>(0);
</script>

<div class="flex flex-row">
	<div class="flex flex-col items-center m-1">
		{label}
		<input type="range" {step} {min} {max} bind:value />
		{value}
	</div>
	<button onclick={() => onChange(value)}>Render</button>
</div>
