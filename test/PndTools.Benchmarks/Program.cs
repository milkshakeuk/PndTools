// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
