import type { Color } from './Color';

export interface ImageFilter {
	apply(image: ImageData): ImageData;
}

export function applyMorphologicalOperation(
	image: ImageData,
	structuringElement: number[][],
	operation: 'min' | 'max'
): ImageData {
	const result = new ImageData(image.width, image.height);
	const useMin = operation === 'min';
	const initialValue = useMin ? 255 : 0;

	for (let y = 0; y < image.height; y++) {
		for (let x = 0; x < image.width; x++) {
			let valueR = initialValue;
			let valueG = initialValue;
			let valueB = initialValue;

			for (let sy = 0; sy < structuringElement.length; sy++) {
				for (let sx = 0; sx < structuringElement[0].length; sx++) {
					const imageX = x + sx - Math.floor(structuringElement[0].length / 2);
					const imageY = y + sy - Math.floor(structuringElement.length / 2);

					if (imageX >= 0 && imageX < image.width && imageY >= 0 && imageY < image.height) {
						const idx = (imageY * image.width + imageX) * 4;
						if (structuringElement[sy][sx] === 1) {
							if (useMin) {
								valueR = Math.min(valueR, image.data[idx]);
								valueG = Math.min(valueG, image.data[idx + 1]);
								valueB = Math.min(valueB, image.data[idx + 2]);
							} else {
								valueR = Math.max(valueR, image.data[idx]);
								valueG = Math.max(valueG, image.data[idx + 1]);
								valueB = Math.max(valueB, image.data[idx + 2]);
							}
						}
					}
				}
			}

			const idx = (y * image.width + x) * 4;
			result.data[idx] = valueR;
			result.data[idx + 1] = valueG;
			result.data[idx + 2] = valueB;
			result.data[idx + 3] = 255;
		}
	}
	return result;
}

export class NoopFilter implements ImageFilter {
	apply(image: ImageData): ImageData {
		return image;
	}
}

export class DilationFilter implements ImageFilter {
	private element: number[][];

	constructor(structuringElement?: number[][]) {
		this.element = structuringElement || [
			[1, 1, 1],
			[1, 1, 1],
			[1, 1, 1]
		];
	}

	apply(image: ImageData): ImageData {
		return applyMorphologicalOperation(image, this.element, 'max');
	}
}

export class ErosionFilter implements ImageFilter {
	private element: number[][];

	constructor(structuringElement?: number[][]) {
		this.element = structuringElement || [
			[1, 1, 1],
			[1, 1, 1],
			[1, 1, 1]
		];
	}

	apply(image: ImageData): ImageData {
		return applyMorphologicalOperation(image, this.element, 'min');
	}
}
export class OpeningFilter implements ImageFilter {
	private element: number[][];

	constructor(structuringElement?: number[][]) {
		this.element = structuringElement || [
			[1, 1, 1],
			[1, 1, 1],
			[1, 1, 1]
		];
	}

	apply(image: ImageData): ImageData {
		const erosion = new ErosionFilter(this.element);
		const dilation = new DilationFilter(this.element);
		return dilation.apply(erosion.apply(image));
	}
}

export class ClosingFilter implements ImageFilter {
	private element: number[][];

	constructor(structuringElement?: number[][]) {
		this.element = structuringElement || [
			[1, 1, 1],
			[1, 1, 1],
			[1, 1, 1]
		];
	}

	apply(image: ImageData): ImageData {
		const dilation = new DilationFilter(this.element);
		const erosion = new ErosionFilter(this.element);
		return erosion.apply(dilation.apply(image));
	}
}

export class HitOrMissFilter implements ImageFilter {
	private element1: number[][];
	private element2: number[][];

	constructor(structuringElement1?: number[][], structuringElement2?: number[][]) {
		this.element1 = structuringElement1 || [
			[0, 0, 0],
			[1, 1, 1],
			[0, 0, 0]
		];
		this.element2 = structuringElement2 || [
			[0, 1, 0],
			[0, 1, 0],
			[0, 1, 0]
		];
	}

	apply(image: ImageData): ImageData {
		const grayscale = new ImageData(image.width, image.height);
		for (let i = 0; i < image.data.length; i += 4) {
			const gray = Math.round(
				image.data[i] * 0.299 + image.data[i + 1] * 0.587 + image.data[i + 2] * 0.114
			);
			grayscale.data[i] = grayscale.data[i + 1] = grayscale.data[i + 2] = gray;
			grayscale.data[i + 3] = 255;
		}

		const erosion1 = new ErosionFilter(this.element1);
		const result1 = erosion1.apply(grayscale);

		const inverted = new ImageData(image.width, image.height);
		for (let i = 0; i < image.data.length; i += 4) {
			const invertedValue = 255 - grayscale.data[i];
			inverted.data[i] = inverted.data[i + 1] = inverted.data[i + 2] = invertedValue;
			inverted.data[i + 3] = 255;
		}

		const erosion2 = new ErosionFilter(this.element2);
		const result2 = erosion2.apply(inverted);

		const result = new ImageData(image.width, image.height);
		for (let i = 0; i < image.data.length; i += 4) {
			const val = Math.min(result1.data[i], result2.data[i]);
			result.data[i] = result.data[i + 1] = result.data[i + 2] = val;
			result.data[i + 3] = 255;
		}

		return result;
	}
}

export class CustomElement implements ImageFilter {
	private element: number[][];

	constructor(
		element: number[][],
		private op: 'min' | 'max'
	) {
		this.element = element;
	}

	apply(image: ImageData): ImageData {
		return applyMorphologicalOperation(image, this.element, this.op);
	}
}
