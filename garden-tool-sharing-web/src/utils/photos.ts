import { uploadToolPhoto } from '../data/tools';

export interface QueuedPhoto {
  id: string;
  file: File;
  caption: string;
}

async function prepareImage(file: File, maxSize = 1600): Promise<Blob> {
  let bitmap: ImageBitmap;
  try {
    bitmap = await createImageBitmap(file, { imageOrientation: 'from-image' });
  } catch {
    throw new Error(`Couldn't read "${file.name}". Try a JPEG or PNG.`);
  }

  const scale = Math.min(1, maxSize / Math.max(bitmap.width, bitmap.height));
  const canvas = document.createElement('canvas');
  canvas.width = Math.round(bitmap.width * scale);
  canvas.height = Math.round(bitmap.height * scale);

  const context = canvas.getContext('2d')!;
  context.fillStyle = '#fff'; // transparent PNGs would otherwise turn black as JPEG
  context.fillRect(0, 0, canvas.width, canvas.height);
  context.drawImage(bitmap, 0, 0, canvas.width, canvas.height);
  bitmap.close();

  return new Promise((resolve, reject) =>
    canvas.toBlob(
      (blob) => (blob ? resolve(blob) : reject(new Error('Could not process the image.'))),
      'image/jpeg',
      0.85,
    ),
  );
}

export async function uploadQueuedPhotos(toolId: number, photos: QueuedPhoto[]): Promise<void> {
  for (const photo of photos) {
    const image = await prepareImage(photo.file);
    await uploadToolPhoto(toolId, image, photo.caption.trim() || 'Photo');
  }
}