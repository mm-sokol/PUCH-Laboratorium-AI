import os
import random
import shutil
from sklearn.model_selection import train_test_split
from imblearn.over_sampling import RandomOverSampler
from imblearn.under_sampling import RandomUnderSampler
from PIL import Image
import numpy as np
from collections import Counter

def balance_dataset(input_dir, output_dir, strategy='oversample', target_samples=None):
    """
    Balansuje zbiór danych używając nadpróbkowania lub podpróbkowania.
    """
    classes = os.listdir(input_dir)
    X = []
    y = []

    for class_name in classes:
        class_path = os.path.join(input_dir, class_name)
        for img_name in os.listdir(class_path):
            X.append(os.path.join(class_path, img_name))
            y.append(class_name)

    if strategy == 'oversample':
        sampler = RandomOverSampler(sampling_strategy='auto' if target_samples is None else target_samples)
    else:
        sampler = RandomUnderSampler(sampling_strategy='auto' if target_samples is None else target_samples)

    x_resampled, y_resampled = sampler.fit_resample(np.array(X).reshape(-1, 1), y)
    print('Resampled dataset shape %s' % Counter(y_resampled))

    for x, y in zip(x_resampled, y_resampled):
        dst_dir = os.path.join(output_dir, y)
        os.makedirs(dst_dir, exist_ok=True)
        shutil.copy(str(x[0]), str(dst_dir))

def split_dataset(input_dir, output_dir, train_ratio=0.7, val_ratio=0.15, test_ratio=0.15):
    """
    Dzieli zbiór danych na podzbiory treningowy, walidacyjny i testowy.
    """
    for class_name in os.listdir(input_dir):
        class_dir = os.path.join(input_dir, class_name)
        images = os.listdir(class_dir)

        train, test = train_test_split(images, test_size=1-train_ratio, random_state=42)
        val, test = train_test_split(test, test_size=test_ratio/(test_ratio + val_ratio), random_state=42)

        for subset, images in [('train', train), ('val', val), ('test', test)]:
            subset_dir = os.path.join(output_dir, subset, class_name)
            os.makedirs(subset_dir, exist_ok=True)
            for img in images:
                shutil.copy(os.path.join(class_dir, img), subset_dir)

def augment_data(input_dir: str, output_dir: str, augmentation_factor=2):
    """
    Przeprowadza prostą augmentację danych.
    """
    for root, _, files in os.walk(input_dir):
        for file in files:
            if file.lower().endswith(('.png', '.jpg', '.jpeg')):
                img_path = os.path.join(root, file)
                img = Image.open(img_path)

                rel_path = os.path.relpath(root, input_dir)
                out_dir = os.path.join(output_dir, rel_path)
                os.makedirs(out_dir, exist_ok=True)

                # Oryginalne zdjęcie
                img = img.convert('RGB')
                img.save(os.path.join(out_dir, file), format='JPEG')

                # Augmentowane zdjęcia
                for i in range(augmentation_factor - 1):
                    aug_img = img.copy()
                    # Losowe przekształcenia
                    if random.choice([True, False]):
                        aug_img = aug_img.transpose(Image.FLIP_LEFT_RIGHT)
                    if random.choice([True, False]):
                        aug_img = aug_img.rotate(random.uniform(-10, 10))

                    aug_img.save(os.path.join(out_dir, f'aug_{i}_{file}'))

def folder_stats(path):
    file_counts = {}
    print(f"\nChecking stats for {path}")
    print("-----------------------------------------------------------")
    for root, _, files in os.walk(path):
        jpg_count = len([f for f in files if f.endswith('.jpg')])
        file_counts[root] = jpg_count
        print(f"{root}: ${jpg_count} .jpg files")
    return file_counts

# Użycie funkcji
input_directory = 'dataset'
balanced_directory = 'balanced'
split_directory = 'split'
augmented_directory = 'augmented'

# Balansowanie danych
# balance_dataset(input_directory, balanced_directory, strategy='oversample')

# Podział na zbiory
# split_dataset(balanced_directory, split_directory)

# Augmentacja danych (opcjonalnie)
# augment_data(split_directory, augmented_directory)


folder_stats(f"{split_directory}/train")
folder_stats(f"{split_directory}/test")
folder_stats(f"{split_directory}/val")