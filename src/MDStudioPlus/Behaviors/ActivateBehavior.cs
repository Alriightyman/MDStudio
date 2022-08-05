﻿using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;

namespace MDStudioPlus.Behaviors
{
    public class ActivateBehavior : Behavior<Window>
    {

        bool isActivated;

        public static readonly DependencyProperty ActivatedProperty =
          DependencyProperty.Register(
            "Activated",
            typeof(bool),
            typeof(ActivateBehavior),
            new PropertyMetadata(OnActivatedChanged)
          );

        public bool Activated
        {
            get { return (bool)GetValue(ActivatedProperty); }
            set { SetValue(ActivatedProperty, value); }
        }

        static void OnActivatedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (ActivateBehavior)dependencyObject;
            if (!behavior.Activated || behavior.isActivated)
                return;
            // The Activated property is set to true but the Activated event (tracked by the
            // isActivated field) hasn't been fired. Go ahead and activate the window.
            if (behavior.AssociatedObject.WindowState == WindowState.Minimized)
                behavior.AssociatedObject.WindowState = WindowState.Normal;
            behavior.AssociatedObject.Activate();
        }

        protected override void OnAttached()
        {
            AssociatedObject.Activated += OnActivated;
            AssociatedObject.Deactivated += OnDeactivated;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Activated -= OnActivated;
            AssociatedObject.Deactivated -= OnDeactivated;
        }

        void OnActivated(object sender, EventArgs eventArgs)
        {
            this.isActivated = true;
            Activated = true;
        }

        void OnDeactivated(object sender, EventArgs eventArgs)
        {
            this.isActivated = false;
            Activated = false;
        }

    }
}
