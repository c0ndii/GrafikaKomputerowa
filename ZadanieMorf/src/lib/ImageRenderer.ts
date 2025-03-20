import { NoopFilter, type ImageFilter } from './FilterProvider';

export class ImageRenderer {
	constructor(
		private img: HTMLImageElement,
		private canvas: HTMLCanvasElement,
		private ctx: CanvasRenderingContext2D
	) {}

	setNewImage(img: HTMLImageElement) {
		this.img = img;
		this.canvas.width = img.naturalWidth;
		this.canvas.height = img.naturalHeight;
	}

	render(filter: ImageFilter = new NoopFilter()) {
		this.ctx.drawImage(this.img, 0, 0, this.canvas.width, this.canvas.height);
		const imgData = this.ctx.getImageData(0, 0, this.canvas.width, this.canvas.height);
		const filteredImgData = filter.apply(imgData);
		this.ctx.putImageData(filteredImgData, 0, 0);
	}
}

